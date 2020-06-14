# Project Apollo: Notes On Development

This document attempts to describe the environment you need to build
and configure in order to enhance and debug a Project Apollo metaverser server.

Building and running this metaverse server is tricky because there
are several applications and services that must link together:
- metaverse-server: presents API for domain and account management;
- ice-server: provides linkage between Interface's for firewall transversal and streaming;
- domain-server: hosts the "land" and avatars that make up a region of the metaverse;
- Interface: the "user interface" that talks to the above services and presents the user's view of the metaverse;

All four of this services run as separate processes, can run on different computers,
and all have to properly link to each other.

## Metaverse-Server

## Ice-Server

## Domain-Server

## Interface

