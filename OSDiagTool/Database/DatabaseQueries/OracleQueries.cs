using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Database.DatabaseQueries {
    class OracleQueries {

        // Use string.format to append parameters
        public string alterSession = @"ALTER SESSION SET CURRENT_SCHEMA = {0}"; // Schema

        public string lockedObjects = @"SELECT * FROM V$LOCKED_OBJECT";

        public string lockedObjects_2 = @"select session_id ""sid"",SERIAL#  ""Serial"", substr(object_name,1,20) ""Object"",   substr(os_user_name,1,10) ""Terminal"",
            substr(oracle_username,1,10) ""Locker"",   nvl(lockwait,'active') ""Wait"",
            decode(locked_mode,     2, 'row share',     3, 'row exclusive',     4, 'share', 5, 'share row exclusive',     6, 'exclusive',  'unknown') ""Lockmode"",   OBJECT_TYPE ""Type""
            FROM   SYS.GV_$LOCKED_OBJECT A,   SYS.ALL_OBJECTS B,   SYS.GV_$SESSION c
            WHERE   A.OBJECT_ID = B.OBJECT_ID AND   C.SID = A.SESSION_ID";

        public string sidInfo = @"SELECT * FROM V$SESSON s JOIN V$PROCESS p ON p.ADDR = s.PADDR WHERE s.SID = {0}"; // {0}: SID

        public string resourceLimit = @"SELECT * FROM V$RESOURCE_LIMIT";

        public string sessionByIOType = @"select p.spid, s.sid, s.process cli_process, s.status, t.{0}, s.last_call_et/3600 last_call_et_Hrs, s.action, s.program, lpad(t.sql_text,30)
            from v$session s, v$sqlarea t, v$process p
            where s.sql_address = t.address
            and s.sql_hash_value = t.hash_value
            and p.addr = s.paddr
            order by t.{0} desc"; // {0}: DISK_READS for Read Throughput || DIRECT_WRITES for Write Throughput

        public string sqlTextBySID = @"select a.sid, a.serial#, a.program, b.sql_text
            from v$session a, v$sqltext b
            where a.sql_hash_value = b.hash_value
            and a.sid = {0}
            order by a.sid,hash_value,piece"; // {0}: SID

        public string tk_queriesRunning = @"
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
