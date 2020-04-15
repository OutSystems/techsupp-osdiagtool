using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string costlyCPUQueries { get; set; } = @"SELECT TOP 20
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
