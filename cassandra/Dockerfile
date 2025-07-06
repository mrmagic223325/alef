FROM cassandra:latest

COPY ./db.cql /docker-entrypoint-initdb.d/

COPY ./entrypoint.sh /

ENTRYPOINT ["/entrypoint.sh"]
CMD ["cassandra", "-f"]
