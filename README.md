# CQRS
![Diagram](https://github.com/user-attachments/assets/4f2f6f08-1cd7-4a9e-8ba8-8079cf95ec4d)

### Debezium Connector
![image](https://github.com/user-attachments/assets/20085f2c-cd58-460f-a8f1-66762d42f1fa)

http://localhost:8083/connectors

`{
    "name": "PostgresConnector",
    "config": {
        "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
        "tasks.max": "1",
        "topic.prefix": "datatransfer",
        "database.hostname": "host.docker.internal",
        "database.port": "5432",
        "database.user": "postgres",
        "database.password": "P@ssw0rd!",
        "database.dbname": "Products",
        "database.server.name": "host.docker.internal",
        "plugin.name": "pgoutput",
        "name": "PostgresConnector"
    },
    "tasks": [],
    "type": "source"
}`

### Gateway
![image](https://github.com/user-attachments/assets/2a17ec5a-e9e1-4f33-93ef-bb5470ff4b5d)

https://localhost:7000/swagger/index.html
