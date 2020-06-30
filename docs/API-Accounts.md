# MetaverseAPI - Account Management

- [Accounts](#Accounts)
- [Account Tokens](#AccountTokens)
- [Domains](#Domains)


## Accounts

## POST /oauth/token

This request mimics and official OAuth request interface. The POST request
sends authentication information and returns an account access token.

The post body is an "applicaton/json" structure that depends on the grant type:

```
    {
        "grant_type": "password",
        "username": username,
        "password": password
    }
```

The following is for a "login" using an external service. Was used for
Steam login. As of 2200621, this is not implemented but kept here for
future addition.

```
    {
        "grant_type": "authorization_code",
        "client_id": stringIDofClient,
        "client_secret": stringSecret,
        "code": stringAuthorizationTokenFromService,
        "redirect_url": loginRedirectionURL
    }
```

Grant type "refresh_token" is used to create a new token that extends the
life of an account's access token. If successful, this returns the refreshed
account access token with an extended expiration time.

```
    {
        "grant_type": "refresh_token",
        "refresh_token": refreshTokenForAccountsAccessToken,
        "scope": "owner"
    }
```

A successful response (HTTP response code "200 OK"), returns an "application/json"
body formatted:

```
    {
        "access_token": tokenString,
        "token_type": "Bearer",
        "expires_in": integerSecondsUntilTokenExpiration,
        "refresh_token": tokenToUseToRefreshThisToken,
        "scope": scopeOfTokenUse,
        "created_at": integerUnixTimeSeconds
    }
```

The failure of the request will return an "application/json" body:

```
    {
        "error": stringDescribingError
    }
```

## GET /api/v1/accounts

Get a list of accounts. The requestor must be logged in and normally it will
return only the user's 'connections'. If the requestor
is an administrator and the URL includes the query "asAdmin", all accounts
are returned (limited by pagination).

- per_page: maximum number of entries to return
- page: the group of "per_page" to return. For instance, if there are 100 users and `per_page=10` and `page=2`, the request will return entries 11 thru 20.
- filter: select type of user. A comma separated list of "connections", "friends"
- status: status of user. A comma separated list of "online"
- search: TODO: figure this one out
- asAdmin: if logged in account is administrator, list all accounts. Value is optional.

```
    {
        "status": "success",
        "data": {
            "accounts": [
                {
                    "accountid": "uniqueAccountId",
                    "username: "username",
                    "email": "email",
                    "public_key": "usersPublicKey",
                    "images": {
                        "hero": stringUrlToImage,
                        "thumbnail": stringUrlToImage,
                        "tiny": stringUrlToImage
                    },
                    "location": {
                        "connected": false,             // whether currently active
                        "path": "X,Y,Z/X,Y,Z,W",
                        "placeid": stringIdOfPlace,
                        "domainid": stringIdOfDomain,
                        "availability": stringWhoCanSee // one of "all", "none", "connections", "friends"
                    },
                    "friends": [ "friendName", "friendName", ... ],
                    "connections": [ "connectionName", "connectionName", ...],
                    "administator": false,
                    "when_account_created": "YYYY-MM-DDTHH:MM:SS.MMMZ",
                    "time_of_last_heartbeat": "YYYY-MM-DDTHH:MM:SS.MMMZ"
                },
                ...
            ]
        }
    }
```

## POST /api/v1/account/{accountId}

Update account information.

## DELETE /api/v1/account/{accountId}

Delete an account.

---

## Account Tokens

## GET /api/v1/account/{accountId}/tokens

Get the tokens held by the account. The requesting account must be
logged in and be either the account identified in "{accountId}" or
be an administrative account.

```
    {
        "status": "success",
        "data": {
            "tokens": [
                {
                    "tokenid": stringTokenIdentifier,
                    "token": stringToken,
                    "refresh_token": stringTokenForRefreshingToken,
                    "token_creation_time": "YYYY-MM-DDTHH:MM:SS.MMMZ",
                    "token_expiration_time": "YYYY-MM-DDTHH:MM:SS.MMMZ",
                    "scope": stringScope    // one of "any", "owner", "domain", "web"
                },
                ...
            ]
        }
    }
```

## DELETE /api/v1/account/{accountId}/tokens/{tokenId}

Delete a particular token held by account.

---

## Domains

## GET /api/v1/domains

## POST /api/v1/domain/%

## DELETE /api/v1/domain/%
