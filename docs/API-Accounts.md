# MetaverseAPI - Account Related Operations

Requests that create and manage accounts.

- [Users](#Users) - Get information on other users
- [Friends](#Friends) - Get information on friends
- [Connections](#Connections) - Get and set connections
- [Administrative](#Administrative) - administrative account requests

## Users

## GET /api/v1/users

- per_page:
- page:
- filter:
- status:
- search:


```
    {
        "status": "success",
        "data": {
            "users": [
                {
                    "username": username,
                    "connection":
                    "images": {
                        "Hero": heroImageURL,
                        "Thumbnail": thumbnailImageURL,
                        "tiny": tinyImageURL
                    },
                    "location": {
                        "node_id": stringSessionId,
                        "root": {
                            "name": stringPlaceName
                        }
                    }
                },
                ...
            ]
        }
    }
```

## GET /api/v1/user/profile

---
## Friends

## GET /api/v1/user/friends


```
    {
        "status": "success",
        "data": {
            "friends": [
                username,
                username,
                ...
            ]
        }
    }
```

## POST /api/v1/user/friends

```
    {
        "username": stringUsername
    }
```

```
    {
        "status": "success"
    }
```

## DELETE /api/v1/user/friends/{username}

```
    {
        "status": "success"
    }
```

---
## Connections

## POST /api/v1/user/connections
## DELETE /api/v1/user/connections/{username}
## GET /api/v1/user/connection_request

---
## Administrative

## PUT /api/v1/user/location
## GET /api/v1/users/{username}/location

## PUT /api/v1/user/public_key
## GET /api/v1/users/{username}/public_key

## POST /api/v1/users

## PUT /api/v1/user/heartbeat


```
    {
        "location": {
        }
    }
```

```
    {
        "status": "success",
        "data": {
            "session_id": stringSessionId
        }
    }
```


## GET /api/v1/user/locker
## POST /api/v1/user/locker

