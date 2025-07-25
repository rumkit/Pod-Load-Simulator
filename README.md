# Pod-Load-Simulator

[![Docker Image CI](https://github.com/rumkit/Pod-Load-Simulator/actions/workflows/docker-image.yml/badge.svg)](https://github.com/rumkit/Pod-Load-Simulator/actions/workflows/docker-image.yml)

## Description
A simple client-server application that allows to simulate CPU and memory utilization on clients (e.g. running as k8s pods, thus the name)

![Server UI](.github/media/podload-server-clients-view.png)

Clients are displayed and can be configured via simple UI on the server

## Build and run

With docker installed launch docker-compose using the command

`docker compose -f src/compose.yaml up -d`

This will build the images and launch a server with 5 clients. Images are built using the multistage Dockerfiles no extra dependencies are required.

Kubernetes specs are available [here](k8s/)

