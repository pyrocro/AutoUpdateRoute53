FROM mono:latest as builder
#RUN mkdir -p release 
WORKDIR /usr/src/app/build/
COPY ["./","./"]
RUN ls /usr/src/app/build/
RUN ls /usr/src/app/build/AutoUpdateRoute53/
RUN nuget restore AutoUpdateRoute53.sln

RUN msbuild AutoUpdateRoute53.sln /p:Configuration=Debug
RUN ls /usr/src/app/build/AutoUpdateRoute53/bin/Debug

#RUN msbuild MjpegProxyServer.sln /p:Configuration=Release
#RUN ls /usr/src/app/build/MjpegProxyServer/bin/Debug

#FROM scratch
FROM mono:latest
ENV DOMAIN_NAME="xxx"
ENV HOSTING_ZONE_ID="xxx"
ENV ACCESS_KEY_ID="xxx"
ENV SECRET_KEY="xxx"
ENV SYNC_EVERY_SECONDS="xxx"
WORKDIR /app/
COPY --from=builder /usr/src/app/build/AutoUpdateRoute53/bin/ ./
RUN pwd
RUN ls ./

#CMD [ "sh",  "-c", "mono /usr/src/app/build/MjpegProxyServer/bin/Debug/MjpegProxyServer.exe" ]
CMD [ "mono","/app/Debug/AutoUpdateRoute53.exe" ]

EXPOSE 80


