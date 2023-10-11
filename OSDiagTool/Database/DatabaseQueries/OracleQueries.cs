namespace OSDiagTool.Database.DatabaseQueries {
    class OracleQueries {

        // Use string.format to append parameters // Sid is always the 1st column
        public string alterSession { get; set; } = @"ALTER SESSION SET CURRENT_SCHEMA = {0}"; // Schema

        public string lockedObjects { get; set; } = @"select session_id, object_id, oracle_username, os_user_name, process, locked_mode, xidsqn, xidslot, xidusn from v$locked_object";

        public string lockedObjects_2 { get; set; } = @"select session_id ""sid"",SERIAL#  ""Serial"", substr(object_name,1,20) ""Object"",   substr(os_user_name,1,10) ""Terminal"",
            substr(oracle_username,1,10) ""Locker"",   nvl(lockwait,'active') ""Wait"",
            decode(locked_mode,     2, 'row share',     3, 'row exclusive',     4, 'share', 5, 'share row exclusive',     6, 'exclusive',  'unknown') ""Lockmode"",   OBJECT_TYPE ""Type""
            FROM   SYS.GV_$LOCKED_OBJECT A,   SYS.ALL_OBJECTS B,   SYS.GV_$SESSION c
            WHERE   A.OBJECT_ID = B.OBJECT_ID AND   C.SID = A.SESSION_ID";

        public string sidInfo { get; set; } = @"SELECT * FROM V$SESSION s JOIN V$PROCESS p ON p.ADDR = s.PADDR WHERE s.SID IN ({0})"; // {0}: SID

        public string resourceLimit { get; set; } = @"SELECT * FROM V$RESOURCE_LIMIT";

        public string sessionByIOType { get; set; } = @"select s.sid, s.process cli_process, s.status, t.{0}, s.action, s.program, lpad(t.sql_text,30)
            from v$session s, v$sqlarea t, v$process p
            where s.sql_address = t.address
            and s.sql_hash_value = t.hash_value
            and p.addr = s.paddr
            order by t.{0} desc"; // {0}: DISK_READS for Read Throughput || DIRECT_WRITES for Write Throughput

        public string sqlTextBySID { get; set; } = @"select a.sid, a.serial#, a.program, b.sql_text
            from v$session a, v$sqltext b
            where a.sql_hash_value = b.hash_value
            and a.sid IN ({0})
            order by a.sid,hash_value,piece"; // {0}: SID

        public string topCPUSqls { get; set; } = @"select * from (select sql_text, cpu_time/1000000 cpu_time,elapsed_time/1000000 elapsed_time, disk_reads, buffer_gets, rows_processed
            from v$sqlarea order by cpu_time desc, disk_reads desc) where rownum < {0}"; // {0}: Number of rows to fetch

        public string tk_queriesRunningNow { get; set; } = @"
            with q as (
            select sql_id from(
             select q.sql_id,sum(disk_reads),sum(buffer_gets),sum(cpu_time),sum(elapsed_time)
             from v$sqlarea q, v$session s
             where s.sql_hash_value = q.hash_value
             and s.sql_address = q.address
             and s.username is not null
             and LAST_ACTIVE_TIME > sysdate - 60/24/60   -- Last Active Time > 60 minutes
             group by q.sql_id
            order by 5 desc,4 desc,2 desc,3 desc)
            where rownum<1000
            )
            SELECT sql_id,lpad(plan_hash_value,15) plan_hash_value,sum(executions) executions,
            round(sum(rows_processed)/(sum(executions) + .0001),2) rows_prsd_per_exec,
            round(sum(disk_reads)/(sum(executions) + .0001),2) phyio_per_exec,
            round(sum(buffer_gets)/(sum(executions) + .0001),2) lio_per_exec,
            round(sum(cpu_time)/1000/(case when sum(executions) =0 then 1 else sum(executions) end),2) cpu_time_per_exec_milisecs,
            round(SUM(elapsed_time)/1000/(case when sum(executions) =0 then 1 else sum(executions) end),2) elap_time_per_exec_milisecs,
            sql_profile, parsing_schema_name,module,sql_text
            FROM gv$sql where sql_id in (
            select sql_id from q
            )
            group by sql_id,plan_hash_value,sql_profile,parsing_schema_name,module,sql_text
            order by 1,2";
    }
}
