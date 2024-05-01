import psycopg2

conn = psycopg2.connect(
    dbname='deanery',
    user='postgres',
    password='pass_deanery',
    host='localhost',
    port=2345
)

cur = conn.cursor()

with open('./Source/deanery.sql', 'r', encoding="utf-8") as file:
    sql_script = file.read()

cur.execute(sql_script)
conn.commit()

cur.close()
conn.close()
