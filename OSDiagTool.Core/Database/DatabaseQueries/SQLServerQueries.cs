namespace OSDiagTool.Database.DatabaseQueries {
    class SQLServerQueries {

        // Use string.format to append parameters

        public string sessionsSp_Who2 { get; set; } = @"SELECT spid, sp.[status], loginame[Login], hostname, blocked BlkBy, sd.name DBName,
            cmd Command, cpu CPUTime, physical_io DiskIO, last_batch LastBatch, [program_name] ProgramName
            FROM master.dbo.sysprocesses sp
            JOIN master.dbo.sysdatabases sd ON sp.dbid = sd.dbid
            ORDER BY cpu DESC";

        public string sessionsSp_Who2_Blocked { get; set; } = @"SELECT spid, blocked BlkBy
            FROM master.dbo.sysprocesses sp
            JOIN master.dbo.sysdatabases sd ON sp.dbid = sd.dbid WHERE blocked <> 0
            ORDER BY cpu DESC";

        public string sessionsLocksTree { get; set; } = @"
            SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

            SET NOCOUNT ON;

            IF OBJECT_ID(N'tempdb..#T1', N'U') IS NOT NULL   
            DROP TABLE #T1;  

            IF OBJECT_ID(N'tempdb..#T2', N'U') IS NOT NULL   
            DROP TABLE #T2;  

            IF OBJECT_ID(N'tempdb..#T3', N'U') IS NOT NULL   
            DROP TABLE #T3;  


            --STEP 1.1 GET BLOCKED CHAINS
            SELECT SPID, BLOCKED, REPLACE(REPLACE(T.TEXT, CHAR(10), ' '), CHAR(13), ' ') AS BATCH
            INTO #T1
            FROM sys.sysprocesses R
            CROSS APPLY sys.dm_exec_sql_text(R.SQL_HANDLE) T;


            WITH BLOCKERS (
	            SPID, BLOCKED, LEVEL, BATCH
	            )
            AS (
	            SELECT SPID, BLOCKED, CAST(REPLICATE('0', 4 - LEN(CAST(SPID AS VARCHAR))) + CAST(SPID AS VARCHAR) AS VARCHAR(1000)) AS LEVEL, BATCH
	            FROM #T1 R
	            WHERE (
			            BLOCKED = 0
			            OR BLOCKED = SPID
			            )
		            AND EXISTS (
			            SELECT *
			            FROM #T1 R2
			            WHERE R2.BLOCKED = R.SPID
				            AND R2.BLOCKED <> R2.SPID
			            )
	            UNION ALL
	            SELECT R.SPID, R.BLOCKED, CAST(BLOCKERS.LEVEL + RIGHT(CAST((1000 + R.SPID) AS VARCHAR(100)), 4) AS VARCHAR(1000)) AS LEVEL, R.BATCH
	            FROM #T1 AS R
	            INNER JOIN BLOCKERS ON R.BLOCKED = BLOCKERS.SPID
	            WHERE R.BLOCKED > 0 AND R.BLOCKED <> R.SPID
	            )
            SELECT CAST(SPID AS NVARCHAR(10)) as SPID, BLOCKED as BlockedBy, LEVEL,
		            N'    ' + REPLICATE(N'|         ', LEN(LEVEL) / 4 - 1) + CASE 
		            WHEN (LEN(LEVEL) / 4 - 1) = 0
			            THEN 'HEAD -  '
		            ELSE '|------  '
		            END + CAST(SPID AS NVARCHAR(10)) + N' ' + BATCH AS BLOCKING_TREE
            --select *
            INTO #T2
            FROM BLOCKERS
            ORDER BY LEVEL ASC;


            --OUTPUT BLOCKED CHAINS
            SELECT SPID, BlockedBy,BLOCKING_TREE,
            convert(varchar,dateadd(ms,WT.wait_duration_ms,0),114) as Wait_Duration,
            Wait_Type
            FROM #T2
            LEFT JOIN sys.dm_os_waiting_tasks AS WT
            on #T2.SPID = WT.session_id
            order by level;


            --STEP 1.2 GET ALL LOCKED RESOURCES IN A DATABASE
            SELECT dm_tran_locks.request_session_id as SPID,
	            convert(varchar,dateadd(ms,WT.wait_duration_ms,0),114) as Wait_Duration
	            ,db_name(resource_database_id) as DatabaseName
	            ,ObjectName = CASE
		            WHEN resource_type = 'OBJECT'
			            THEN QUOTENAME(OBJECT_SCHEMA_NAME(dm_tran_locks.resource_associated_entity_id)) + N'.' + QUOTENAME(OBJECT_NAME(dm_tran_locks.resource_associated_entity_id))
		            ELSE QUOTENAME(OBJECT_SCHEMA_NAME(partitions.OBJECT_ID)) + N'.' + QUOTENAME(OBJECT_NAME(partitions.OBJECT_ID))
		            END 
	            ,partitions.index_id, indexes.name AS index_name, dm_tran_locks.resource_type, dm_tran_locks.resource_description
	            ,dm_tran_locks.resource_associated_entity_id, dm_tran_locks.request_mode, dm_tran_locks.request_status
	            ,GET_RECORDS = CASE   
		            WHEN resource_type = 'OBJECT' then 'select * from ' + QUOTENAME(db_name(resource_database_id)) + N'.' + QUOTENAME(OBJECT_NAME(dm_tran_locks.resource_associated_entity_id)) + ' with (nolock)'
		            WHEN resource_type = 'PAGE' then 'select sys.fn_PhysLocFormatter (%%physloc%%) as resource_description,* from ' + QUOTENAME(db_name(resource_database_id)) + N'.' + QUOTENAME(OBJECT_SCHEMA_NAME(partitions.OBJECT_ID)) + N'.' + QUOTENAME(OBJECT_NAME(partitions.OBJECT_ID)) + ' (nolock) where sys.fn_PhysLocFormatter (%%physloc%%) like ''%' + REPLACE(dm_tran_locks.resource_description, ' ', '') + '%'''
		            WHEN resource_type != 'PAGE' and partitions.index_id <= 1 then 'select * from ' + QUOTENAME(db_name(resource_database_id)) + N'.' + QUOTENAME(OBJECT_SCHEMA_NAME(partitions.OBJECT_ID)) + N'.' + QUOTENAME(OBJECT_NAME(partitions.OBJECT_ID)) + ' with (nolock) where %%lockres%% = ''' + REPLACE(dm_tran_locks.resource_description, ' ', '') + ''''
		            WHEN resource_type != 'PAGE' and partitions.index_id > 1 then 'select * from ' + QUOTENAME(db_name(resource_database_id)) + N'.' + QUOTENAME(OBJECT_SCHEMA_NAME(partitions.OBJECT_ID)) + N'.' + QUOTENAME(OBJECT_NAME(partitions.OBJECT_ID)) + ' with (nolock index(' + indexes.name COLLATE DATABASE_DEFAULT + ')) where %%lockres%% = ''' + REPLACE(dm_tran_locks.resource_description, ' ', '') + ''''
		            ELSE ''
	            END
	            --,QUOTENAME(OBJECT_SCHEMA_NAME(dm_tran_locks.resource_associated_entity_id)) + N'.' + QUOTENAME(OBJECT_NAME(dm_tran_locks.resource_associated_entity_id) as t
            INTO #T3
            FROM sys.dm_tran_locks
            LEFT JOIN sys.partitions ON partitions.hobt_id = dm_tran_locks.resource_associated_entity_id
            LEFT JOIN sys.indexes ON indexes.OBJECT_ID = partitions.OBJECT_ID
	            AND indexes.index_id = partitions.index_id
            LEFT JOIN sys.dm_os_waiting_tasks AS WT
             ON sys.dm_tran_locks.lock_owner_address = WT.resource_address
            WHERE resource_associated_entity_id > 0
            --and request_status = 'wait'
            --	AND resource_database_id = DB_ID()
              AND request_session_id in(SELECT SPID FROM #T2)
            --ORDER BY resource_associated_entity_id,request_session_id;


            IF (SELECT COUNT(1) FROM #T3 WHERE ObjectName is not null) = 0 AND (SELECT COUNT(1) FROM #T3) > 0
	            SELECT 'Using [' + DB_NAME() + '] -For more details, execute again in the database below' as 'Information Message';
                ";

        public string locksAssociatedResources { get; set; } = @"
            SELECT * from #T3 ORDER BY resource_associated_entity_id,SPID;

            --CLEAN UP TEMP TABLES
            DROP TABLE #T1;

            DROP TABLE #T2;

            DROP TABLE #T3;";

        public string costlyCPUQueries { get; set; } = @"SELECT TOP {0}
            last_execution_time, qs.execution_count,
            total_CPU_inSeconds = --Converted from microseconds
                qs.total_worker_time/1000000,
            average_CPU_inSeconds = --Converted from microseconds
                (qs.total_worker_time/1000000) / qs.execution_count,
            total_elapsed_time_inSeconds = --Converted from microseconds
                qs.total_elapsed_time/1000000,
            st.text
            FROM
                sys.dm_exec_query_stats AS qs
            CROSS APPLY
                sys.dm_exec_sql_text(qs.sql_handle) AS st
            CROSS APPLY
                sys.dm_exec_query_plan (qs.plan_handle) AS qp
            ORDER BY
                qs.total_worker_time DESC";

        public string dbccInputBuffer { get; set; } = @"SELECT es.session_id as spid, ib.event_info, status,
            cpu_time, memory_usage, logical_reads, writes, row_count
            total_elapsed_time, login_time, last_request_start_time, last_request_end_time
            host_name, program_name, login_name, open_transaction_count
            FROM sys.dm_exec_sessions AS es
            CROSS APPLY sys.dm_exec_input_buffer(es.session_id, NULL) AS ib
            WHERE es.session_id IN({0})"; // {0}: SID

        public string statCachedPlans { get; set; } = @"SELECT /* TOOLKIT */ TOP {0}
            qs.last_execution_time AS ""Last Execution Date""
            ,qs.execution_count AS ""Execution Count""
            , DB_NAME(qt.dbid) AS ""DB Name""
            , CONVERT(DECIMAL(10, 2), qs.total_worker_time / 1000.0) AS ""Total CPU Time(ms)""
            --,(qs.total_worker_time/qs.execution_count)/1000.0 AS ""Avg CPU Time(ms)""                      -- Converted from miliseconds
            ,qs.total_physical_reads AS ""Total Physical Reads""
            --,qs.total_physical_reads/qs.execution_count AS ""Avg Physical Reads""
            ,qs.total_logical_reads AS ""Total Logical Reads""
            --,qs.total_logical_reads/qs.execution_count AS ""Avg Logical Reads""
            ,qs.total_logical_writes AS ""Total Logical Writes""
            --,qs.total_logical_writes/qs.execution_count AS ""Avg Logical Writes""
            ,CONVERT(DECIMAL(10,2),qs.total_elapsed_time/1000.0) AS ""Total Duration(ms)""
            ,CONVERT(DECIMAL(10,2),(qs.total_elapsed_time/qs.execution_count)/1000) AS ""Avg Duration(ms)""  -- Converted from miliseconds
            ,CONVERT(DECIMAL(10,2),qs.last_worker_time/1000.0) AS ""Last CPU Time(ms)""
            ,CONVERT(DECIMAL(10,2),qs.last_elapsed_time/1000.0) AS ""last Duration(ms)""
            ,qs.query_hash
            ,qs.query_plan_hash
            ,qs.plan_handle
            ,qs.sql_handle
            ,SUBSTRING(qt.text, qs.statement_start_offset/2 +1, (CASE WHEN qs.statement_end_offset = -1 THEN LEN(CONVERT(NVARCHAR(MAX), qt.text)) * 2 ELSE qs.statement_end_offset END - qs.statement_start_offset)/2) AS ""Query Text""
            , qp.query_plan AS ""Plan""
        FROM sys.dm_exec_query_stats AS qs
        CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS qt
        CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
        WHERE 1=1
            --AND qs.query_hash= 0xEA5E7DEE39CB64B1
            --AND qs.execution_count > 50
            --AND qs.total_worker_time/qs.execution_count > 1000000      -- >1000 miliseconds or 1 second
            --AND qs.total_physical_reads/qs.execution_count > 1000
            --AND qs.total_logical_reads/qs.execution_count > 1000
            --AND qs.total_logical_writes/qs.execution_count > 1000
            --AND qs.total_elapsed_time/qs.execution_count > 1000000         -- >1000 miliseconds or 1 second
            --AND qs.last_physical_reads > 0
            --AND qs.last_physical_writes > 0
            --AND last_logical_reads > 0
            --AND last_logical_writes > 0
            --AND qs.last_worker_time > 1000000                          --> 1000 miliseconds or 1 second
            --AND qs.last_elapsed_time > 1000000                             --> 1000 miliseconds or 1 second
            --AND SUBSTRING(qt.text, qs.statement_start_offset/2 +1, (CASE WHEN qs.statement_end_offset = -1 THEN LEN(CONVERT(NVARCHAR(MAX), qt.text)) * 2 ELSE qs.statement_end_offset END - qs.statement_start_offset)/2) NOT LIKE 'SELECT /* TOOLKIT */%'
            --AND cast(qs.last_execution_time as date) = cast(getDate()-2 As Date)
            AND qs.last_execution_time > dateadd(hh,-1, getdate())                -- last hour
            --AND qs.last_execution_time > dateadd(minute,-10, getdate())         -- last 10 minutes
         ORDER BY
             qs.last_execution_time DESC, qs.last_elapsed_time DESC
            --qs.execution_count DESC
            --qs.total_elapsed_time/qs.execution_count DESC
            --qs.total_worker_time/qs.execution_count DESC
            --qs.last_execution_time DESC, qs.last_worker_time DESC
            --qs.total_physical_reads/qs.execution_count DESC
            --qs.total_logical_reads/qs.execution_count DESC
            --qs.total_logical_writes/qs.execution_count DESC
            --qs.last_elapsed_time DESC
            --qs.last_worker_time DESC"; // {0}: TOP records
    }
}
