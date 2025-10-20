"""OdMast"""
import os
from cassandra.cluster import Cluster
from cassandra.query import BatchStatement, ConsistencyLevel
from cassandra import InvalidRequest
scylla_host = "127.0.0.1"
port = 9042
cluster = Cluster([scylla_host], port=port)
session = cluster.connect()
session.set_keyspace("scarlight")  # Thay bằng tên keyspace của bạn
COLUMNS = [
    "actype", "orderid", "codeid", "afacctno", "seacctno", "ciacctno", "txnum", "txdate", "txtime", "expdate", "bratio",
    "timetype", "exectype", "nork", "matchtype", "via", "clearday", "clearcd", "orstatus", "pricetype", "quoteprice",
    "stopprice", "limitprice", "orderqtty", "remainqtty", "execqtty", "standqtty", "cancelqtty", "adjustqtty",
    "rejectqtty", "rejectcd", "custid", "exprice", "exqtty", "iccfcd", "iccftied", "execamt", "examt", "feeamt",
    "consultant", "voucher", "odtype", "feeacr", "porstatus", "rlssecured", "securedamt", "matchamt", "deltd",
    "reforderid", "banktrfamt", "banktrffee", "edstatus", "correctionnumber", "contrafirm", "traderid", "clientid",
    "confirm_no", "foacctno", "hosesession", "contraorderid", "puttype", "contrafrm", "dfacctno", "last_change",
    "dfqtty", "stsstatus", "feebratio", "tlid", "ssafacctno", "advidref", "noe", "grporder", "grpamt", "excfeeamt",
    "excfeerefid", "isdisposal", "taxrate", "taxsellamt", "errod", "errsts", "errreason", "ferrod", "fixerrtype",
    "errodref", "quoteqtty", "confirmed", "exstatus", "ptdeal", "cancelstatus", "feedbackmsg", "blorderid", "isblorder",
    "subacctno", "odtimestamp", "resendbl", "corebank", "feetranamt", "feeserviceamt", "feeserbfvat", "feeservat",
    "feeratetran", "onlconfirmsts", "direct", "repotype"
]
NUM_COLUMNS = len(COLUMNS)
def parse_sql_file(filename):
    import re
    with open(filename, encoding="utf-8") as f:
        content = f.read()
    pattern = re.compile(r"VALUES\s*\((.*?)\);", re.DOTALL | re.IGNORECASE)
    matches = pattern.findall(content)
    rows = []
    for idx, match in enumerate(matches):
        values = []
        current = ''
        in_string = False
        for c in match:
            if c == "'":
                in_string = not in_string
                current += c
            elif c == "," and not in_string:
                values.append(current.strip())
                current = ''
            else:
                current += c
        if current:
            values.append(current.strip())
        if len(values) != NUM_COLUMNS:
            print(f"[WARNING] Bỏ qua dòng {idx+1} vì số lượng giá trị ({len(values)}) không khớp số cột ({NUM_COLUMNS})")
            continue
        values = [None if v.upper() == "NULL" else v.strip("'") for v in values]
        rows.append(values)
    for row in rows:
        row[COLUMNS.index("bratio")] = float(row[COLUMNS.index("bratio")]) if row[COLUMNS.index("bratio")] else None
        row[COLUMNS.index("clearday")] = int(row[COLUMNS.index("clearday")]) if row[COLUMNS.index("clearday")] else None
        row[COLUMNS.index("correctionnumber")] = int(row[COLUMNS.index("correctionnumber")]) if row[COLUMNS.index("correctionnumber")] else None
        row[COLUMNS.index("feebratio")] = float(row[COLUMNS.index("feebratio")]) if row[COLUMNS.index("feebratio")] else None
        row[COLUMNS.index("taxrate")] = float(row[COLUMNS.index("taxrate")]) if row[COLUMNS.index("taxrate")] else None
        row[COLUMNS.index("orderqtty")] = float(row[COLUMNS.index("orderqtty")]) if row[COLUMNS.index("orderqtty")] else None
        row[COLUMNS.index("remainqtty")] = float(row[COLUMNS.index("remainqtty")]) if row[COLUMNS.index("remainqtty")] else None
        row[COLUMNS.index("execqtty")] = float(row[COLUMNS.index("execqtty")]) if row[COLUMNS.index("execqtty")] else None
        row[COLUMNS.index("standqtty")] = float(row[COLUMNS.index("standqtty")]) if row[COLUMNS.index("standqtty")] else None
        row[COLUMNS.index("cancelqtty")] = float(row[COLUMNS.index("cancelqtty")]) if row[COLUMNS.index("cancelqtty")] else None
        row[COLUMNS.index("adjustqtty")] = float(row[COLUMNS.index("adjustqtty")]) if row[COLUMNS.index("adjustqtty")] else None
        row[COLUMNS.index("rejectqtty")] = float(row[COLUMNS.index("rejectqtty")]) if row[COLUMNS.index("rejectqtty")] else None
        row[COLUMNS.index("exprice")] = float(row[COLUMNS.index("exprice")]) if row[COLUMNS.index("exprice")] else None
        row[COLUMNS.index("exqtty")] = float(row[COLUMNS.index("exqtty")]) if row[COLUMNS.index("exqtty")] else None
        row[COLUMNS.index("execamt")] = float(row[COLUMNS.index("execamt")]) if row[COLUMNS.index("execamt")] else None
        row[COLUMNS.index("examt")] = float(row[COLUMNS.index("examt")]) if row[COLUMNS.index("examt")] else None
        row[COLUMNS.index("feeamt")] = float(row[COLUMNS.index("feeamt")]) if row[COLUMNS.index("feeamt")] else None
        row[COLUMNS.index("rlssecured")] = float(row[COLUMNS.index("rlssecured")]) if row[COLUMNS.index("rlssecured")] else None
        row[COLUMNS.index("securedamt")] = float(row[COLUMNS.index("securedamt")]) if row[COLUMNS.index("securedamt")] else None
        row[COLUMNS.index("matchamt")] = float(row[COLUMNS.index("matchamt")]) if row[COLUMNS.index("matchamt")] else None
        row[COLUMNS.index("banktrfamt")] = float(row[COLUMNS.index("banktrfamt")]) if row[COLUMNS.index("banktrfamt")] else None
        row[COLUMNS.index("banktrffee")] = float(row[COLUMNS.index("banktrffee")]) if row[COLUMNS.index("banktrffee")] else None
        row[COLUMNS.index("dfqtty")] = float(row[COLUMNS.index("dfqtty")]) if row[COLUMNS.index("dfqtty")] else None
        row[COLUMNS.index("feebratio")] = float(row[COLUMNS.index("feebratio")]) if row[COLUMNS.index("feebratio")] else None
        row[COLUMNS.index("grpamt")] = float(row[COLUMNS.index("grpamt")]) if row[COLUMNS.index("grpamt")] else None
        row[COLUMNS.index("taxsellamt")] = float(row[COLUMNS.index("taxsellamt")]) if row[COLUMNS.index("taxsellamt")] else None
        row[COLUMNS.index("quoteqtty")] = float(row[COLUMNS.index("quoteqtty")]) if row[COLUMNS.index("quoteqtty")] else None
        row[COLUMNS.index("feetranamt")] = float(row[COLUMNS.index("feetranamt")]) if row[COLUMNS.index("feetranamt")] else None
        row[COLUMNS.index("feeserviceamt")] = float(row[COLUMNS.index("feeserviceamt")]) if row[COLUMNS.index("feeserviceamt")] else None
        row[COLUMNS.index("feeserbfvat")] = float(row[COLUMNS.index("feeserbfvat")]) if row[COLUMNS.index("feeserbfvat")] else None
        row[COLUMNS.index("feeservat")] = float(row[COLUMNS.index("feeservat")]) if row[COLUMNS.index("feeservat")] else None
        row[COLUMNS.index("feeratetran")] = float(row[COLUMNS.index("feeratetran")]) if row[COLUMNS.index("feeratetran")] else None
        row[COLUMNS.index("resendbl")] = int(row[COLUMNS.index("resendbl")]) if row[COLUMNS.index("resendbl")] else None
        row[COLUMNS.index("repotype")] = int(row[COLUMNS.index("repotype")]) if row[COLUMNS.index("repotype")] else None
    return rows
def write_error_log(row, index, error_msg):
    with open("insert_ODMAST_error.log", "a", encoding="utf-8") as f:
        f.write(f"[Row {index}] ERROR: {error_msg}\n{row}\n\n")
def insert_batch_recursive(prepared, rows, indices):
    try:
        batch = BatchStatement(consistency_level=ConsistencyLevel.QUORUM)
        for row in rows:
            batch.add(prepared, row)
        session.execute(batch)
        print(f"✅ Inserted rows: {indices}")
    except Exception as e:
        if len(rows) == 1:
            idx = indices[0]
            write_error_log(rows[0], idx, str(e))
            print(f"❌ Failed to insert row {idx}: {e}")
        else:
            mid = len(rows) // 2
            insert_batch_recursive(prepared, rows[:mid], indices[:mid])
            insert_batch_recursive(prepared, rows[mid:], indices[mid:])
def bulk_insert(rows, batch_size=100):
    insert_stmt = f"""
    INSERT INTO odmast (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)
    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)
    print("⚡ Bulk insert finished. Check insert_ODMAST_error.log for any failed rows.")
if __name__ == "__main__":
    if os.path.exists("insert_ODMAST_error.log"):
        os.remove("insert_ODMAST_error.log")
    rows = parse_sql_file("odm.sql")
    bulk_insert(rows)





"""OdMastHist"""
import os
from cassandra.cluster import Cluster
from cassandra.query import BatchStatement, ConsistencyLevel
from cassandra import InvalidRequest
scylla_host = "127.0.0.1"
port = 9042
cluster = Cluster([scylla_host], port=port)
session = cluster.connect()
session.set_keyspace("scarlight")  # Thay bằng tên keyspace của bạn
COLUMNS = [
    "actype", "orderid", "codeid", "afacctno", "seacctno", "ciacctno", "txnum", "txdate", "txtime", "expdate", "bratio",
    "timetype", "exectype", "nork", "matchtype", "via", "clearday", "clearcd", "orstatus", "pricetype", "quoteprice",
    "stopprice", "limitprice", "orderqtty", "remainqtty", "execqtty", "standqtty", "cancelqtty", "adjustqtty",
    "rejectqtty", "rejectcd", "custid", "exprice", "exqtty", "iccfcd", "iccftied", "execamt", "examt", "feeamt",
    "consultant", "voucher", "odtype", "feeacr", "porstatus", "rlssecured", "securedamt", "matchamt", "deltd",
    "reforderid", "banktrfamt", "banktrffee", "edstatus", "correctionnumber", "contrafirm", "traderid", "clientid",
    "confirm_no", "foacctno", "hosesession", "contraorderid", "puttype", "contrafrm", "dfacctno", "last_change",
    "dfqtty", "stsstatus", "feebratio", "tlid", "ssafacctno", "advidref", "noe", "grporder", "grpamt", "excfeeamt",
    "excfeerefid", "isdisposal", "taxrate", "taxsellamt", "errod", "errsts", "errreason", "ferrod", "fixerrtype",
    "errodref", "quoteqtty", "confirmed", "exstatus", "ptdeal", "cancelstatus", "feedbackmsg", "blorderid", "isblorder",
    "subacctno", "odtimestamp", "resendbl", "corebank", "feetranamt", "feeserviceamt", "feeserbfvat", "feeservat",
    "feeratetran", "onlconfirmsts", "direct", "repotype"
]
NUM_COLUMNS = len(COLUMNS)
def parse_sql_file(filename):
    import re
    with open(filename, encoding="utf-8") as f:
        content = f.read()
    pattern = re.compile(r"VALUES\s*\((.*?)\);", re.DOTALL | re.IGNORECASE)
    matches = pattern.findall(content)
    rows = []
    for idx, match in enumerate(matches):
        values = []
        current = ''
        in_string = False
        for c in match:
            if c == "'":
                in_string = not in_string
                current += c
            elif c == "," and not in_string:
                values.append(current.strip())
                current = ''
            else:
                current += c
        if current:
            values.append(current.strip())
        if len(values) != NUM_COLUMNS:
            print(f"[WARNING] Bỏ qua dòng {idx+1} vì số lượng giá trị ({len(values)}) không khớp số cột ({NUM_COLUMNS})")
            continue
        values = [None if v.upper() == "NULL" else v.strip("'") for v in values]
        rows.append(values)
    for row in rows:
        row[COLUMNS.index("bratio")] = float(row[COLUMNS.index("bratio")]) if row[COLUMNS.index("bratio")] else None
        row[COLUMNS.index("clearday")] = int(row[COLUMNS.index("clearday")]) if row[COLUMNS.index("clearday")] else None
        row[COLUMNS.index("correctionnumber")] = int(row[COLUMNS.index("correctionnumber")]) if row[COLUMNS.index("correctionnumber")] else None
        row[COLUMNS.index("feebratio")] = float(row[COLUMNS.index("feebratio")]) if row[COLUMNS.index("feebratio")] else None
        row[COLUMNS.index("taxrate")] = float(row[COLUMNS.index("taxrate")]) if row[COLUMNS.index("taxrate")] else None
        row[COLUMNS.index("orderqtty")] = float(row[COLUMNS.index("orderqtty")]) if row[COLUMNS.index("orderqtty")] else None
        row[COLUMNS.index("remainqtty")] = float(row[COLUMNS.index("remainqtty")]) if row[COLUMNS.index("remainqtty")] else None
        row[COLUMNS.index("execqtty")] = float(row[COLUMNS.index("execqtty")]) if row[COLUMNS.index("execqtty")] else None
        row[COLUMNS.index("standqtty")] = float(row[COLUMNS.index("standqtty")]) if row[COLUMNS.index("standqtty")] else None
        row[COLUMNS.index("cancelqtty")] = float(row[COLUMNS.index("cancelqtty")]) if row[COLUMNS.index("cancelqtty")] else None
        row[COLUMNS.index("adjustqtty")] = float(row[COLUMNS.index("adjustqtty")]) if row[COLUMNS.index("adjustqtty")] else None
        row[COLUMNS.index("rejectqtty")] = float(row[COLUMNS.index("rejectqtty")]) if row[COLUMNS.index("rejectqtty")] else None
        row[COLUMNS.index("exprice")] = float(row[COLUMNS.index("exprice")]) if row[COLUMNS.index("exprice")] else None
        row[COLUMNS.index("exqtty")] = float(row[COLUMNS.index("exqtty")]) if row[COLUMNS.index("exqtty")] else None
        row[COLUMNS.index("execamt")] = float(row[COLUMNS.index("execamt")]) if row[COLUMNS.index("execamt")] else None
        row[COLUMNS.index("examt")] = float(row[COLUMNS.index("examt")]) if row[COLUMNS.index("examt")] else None
        row[COLUMNS.index("feeamt")] = float(row[COLUMNS.index("feeamt")]) if row[COLUMNS.index("feeamt")] else None
        row[COLUMNS.index("rlssecured")] = float(row[COLUMNS.index("rlssecured")]) if row[COLUMNS.index("rlssecured")] else None
        row[COLUMNS.index("securedamt")] = float(row[COLUMNS.index("securedamt")]) if row[COLUMNS.index("securedamt")] else None
        row[COLUMNS.index("matchamt")] = float(row[COLUMNS.index("matchamt")]) if row[COLUMNS.index("matchamt")] else None
        row[COLUMNS.index("banktrfamt")] = float(row[COLUMNS.index("banktrfamt")]) if row[COLUMNS.index("banktrfamt")] else None
        row[COLUMNS.index("banktrffee")] = float(row[COLUMNS.index("banktrffee")]) if row[COLUMNS.index("banktrffee")] else None
        row[COLUMNS.index("dfqtty")] = float(row[COLUMNS.index("dfqtty")]) if row[COLUMNS.index("dfqtty")] else None
        row[COLUMNS.index("feebratio")] = float(row[COLUMNS.index("feebratio")]) if row[COLUMNS.index("feebratio")] else None
        row[COLUMNS.index("grpamt")] = float(row[COLUMNS.index("grpamt")]) if row[COLUMNS.index("grpamt")] else None
        row[COLUMNS.index("taxsellamt")] = float(row[COLUMNS.index("taxsellamt")]) if row[COLUMNS.index("taxsellamt")] else None
        row[COLUMNS.index("quoteqtty")] = float(row[COLUMNS.index("quoteqtty")]) if row[COLUMNS.index("quoteqtty")] else None
        row[COLUMNS.index("feetranamt")] = float(row[COLUMNS.index("feetranamt")]) if row[COLUMNS.index("feetranamt")] else None
        row[COLUMNS.index("feeserviceamt")] = float(row[COLUMNS.index("feeserviceamt")]) if row[COLUMNS.index("feeserviceamt")] else None
        row[COLUMNS.index("feeserbfvat")] = float(row[COLUMNS.index("feeserbfvat")]) if row[COLUMNS.index("feeserbfvat")] else None
        row[COLUMNS.index("feeservat")] = float(row[COLUMNS.index("feeservat")]) if row[COLUMNS.index("feeservat")] else None
        row[COLUMNS.index("feeratetran")] = float(row[COLUMNS.index("feeratetran")]) if row[COLUMNS.index("feeratetran")] else None
        row[COLUMNS.index("resendbl")] = int(row[COLUMNS.index("resendbl")]) if row[COLUMNS.index("resendbl")] else None
        row[COLUMNS.index("repotype")] = int(row[COLUMNS.index("repotype")]) if row[COLUMNS.index("repotype")] else None
    return rows
def write_error_log(row, index, error_msg):
    with open("insert_ODMASTHISTORY_error.log", "a", encoding="utf-8") as f:
        f.write(f"[Row {index}] ERROR: {error_msg}\n{row}\n\n")
def insert_batch_recursive(prepared, rows, indices):
    try:
        batch = BatchStatement(consistency_level=ConsistencyLevel.QUORUM)
        for row in rows:
            batch.add(prepared, row)
        session.execute(batch)
        print(f"✅ Inserted rows: {indices}")
    except Exception as e:
        if len(rows) == 1:
            idx = indices[0]
            write_error_log(rows[0], idx, str(e))
            print(f"❌ Failed to insert row {idx}: {e}")
        else:
            mid = len(rows) // 2
            insert_batch_recursive(prepared, rows[:mid], indices[:mid])
            insert_batch_recursive(prepared, rows[mid:], indices[mid:])
def bulk_insert(rows, batch_size=100):
    insert_stmt = f"""
    INSERT INTO odmasthist (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)
    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)
    print("⚡ Bulk insert finished. Check insert_ODMASTHISTORY_error.log for any failed rows.")
if __name__ == "__main__":
    if os.path.exists("insert_ODMASTHIST_error.log"):
        os.remove("insert_ODMASTHIST_error.log")
    rows = parse_sql_file("odmhist.sql")
    bulk_insert(rows)