# Walacor .Net SDK

<div align="center">

<img src="https://www.walacor.com/wp-content/uploads/2022/09/Walacor_Logo_Tag.png" width="300" />

[![License Apache 2.0][badge-license]][license]
[![Walacor (1100127456347832400)](https://img.shields.io/badge/My-Discord-%235865F2.svg?label=Walacor)](https://discord.gg/BaEWpsg8Yc)
[![Walacor (1100127456347832400)](https://img.shields.io/static/v1?label=Walacor&message=LinkedIn&color=blue)](https://www.linkedin.com/company/walacor/)
[![Walacor (1100127456347832400)](https://img.shields.io/static/v1?label=Walacor&message=Website&color)](https://www.walacor.com/product/)

</div>

[badge-license]: https://img.shields.io/badge/license-Apache2-green.svg?dummy
[license]: https://github.com/walacor/objectvalidation/blob/main/LICENSE


A lightweight C#/.NET SDK that wraps Walacor’s REST API and removes the usual boilerplate:
authentication, headers, retries, and common API calls (schemas, data/envelopes, files).

If you’re a .NET developer, the goal is: **create one `WalacorService` and use clean C# methods** instead of manually building HTTP requests.

---

## Features

- **One-liner setup** – create a single `WalacorService` instance and access sub-services.
- **Authentication built-in** – logs in using username/password and caches the token in memory.
- **Auto token refresh** – if a request returns 401, it refreshes the token and retries once.
- **Retries for transient failures** – GET/HEAD calls can retry on 408/429/5xx (configurable).
- **Correlation ID per request** – each request has an `X-Request-ID` for debugging and tracing.
- **File helpers** – verify/store/download flows + basic MIME type guessing.

---

## Installation
### Option A — From GitHub Packages (if you consume packages)

This SDK is published on **NuGet.org** as:

- **Package Id:** `walacor-dotnet-sdk`

### Install (recommended)

```bash
dotnet add package walacor-dotnet-sdk
```
Install a specific version

```bash
dotnet add package walacor-dotnet-sdk --version 0.0.1
```
### Option B — From source (recommended for collaborators)

```bash
git clone https://github.com/walacor/dotnet-sdk.git
cd dotnet-sdk
dotnet build
dotnet test
```
## Supported frameworks

The package targets **.NET Standard 2.0**, which means it runs on a wide range of .NET runtimes, including:

- **.NET 5 / 6 / 7 / 8 / 9**
- **.NET Core 2.0+** (and newer)
- **.NET Framework 4.8.**

---

## Quick Start

### 1) Create a service instance

> !Important: pass the **server base URL without `/api`**.
> The SDK adds `/api/` internally.

```csharp
using Walacor_SDK;

using var wal = new WalacorService(
    baseUri: "https://your-walacor-host/",
    userName: "Admin",
    password: "Password!",
);
```

---

## What’s inside (services)

The main entry point is `WalacorService`, which exposes:

* `wal.SchemaService`
* `wal.DataRequestsService`
* `wal.FileRequestsService`

Each service wraps a part of the API and returns a `Result<T>` object so you can handle success/failure consistently.

---

## Schemas (SchemaService)

Examples of what you can do:

* list schema versions
* list latest schema versions for each envelope type
* get schema details
* create schema (where supported)

```csharp
 var versions = await wal.SchemaService.GetVersionsAsync();
 if (!versions.IsSuccess)
 {
     Console.WriteLine(versions.Error?.Message);
     return;
 }

 foreach (var v in versions.Value)
 {
     Console.WriteLine($"{v.ETId} -> SV {v.Versions[0]}");
 }
```

---

## Data / Envelopes (DataRequestsService)

This service covers submitting and querying envelope records.

### Insert one record

```csharp
var record = new
{
    Name = "Example",
    Value = 123
};

var insert = await wal.DataRequestsService.InsertSingleRecordAsync(record, etId: 12345);
if (!insert.IsSuccess)
{
    Console.WriteLine(insert.Error?.Message);
    return;
}

Console.WriteLine("Inserted successfully.");
```

### Query (simple)

```csharp
var rows = await wal.DataRequestsService.GetAllAsync(etId: 12345, pageNumber: 0, pageSize: 50);
if (rows.IsSuccess)
{
    Console.WriteLine($"Rows: {rows.Value.Count}");
}
```

### Update requires UID

The SDK enforces that updates contain `UID`:

```csharp
var updateRecord = new Dictionary<string, object>
{
    ["UID"] = "some-uid",
    ["Name"] = "Updated"
};

var update = await wal.DataRequestsService.UpdateSingleRecordWithUidAsync(updateRecord, etId: 12345);
```

---

## Files (FileRequestsService)

Typical flow:

1. **Verify** a local file
2. **Store** verified metadata
3. **Download** by UID
4. Optionally **list files**

### Verify a file

```csharp
using Walacor_SDK.Models.FileRequests.Request;

var verify = await wal.FileRequestsService.VerifyAsync(new VerifySingleFileRequest
{
    Path = @"C:\files\report.pdf",
    FileName = "report.pdf"
});

if (!verify.IsSuccess)
{
    Console.WriteLine(verify.Error?.Message);
    return;
}

Console.WriteLine($"Hash: {verify.Value.Hash}");
```

### Store verified metadata

```csharp
var store = await wal.FileRequestsService.StoreAsync(verify.Value.FileInfo);
if (!store.IsSuccess)
{
    Console.WriteLine(store.Error?.Message);
    return;
}

Console.WriteLine($"Stored UID: {store.Value.UID}");
```

### Download

```csharp
var download = await wal.FileRequestsService.DownloadAsync(uid: store.Value.UID);

// By default saves to: ~/Downloads/Walacor/<UID>
if (download.IsSuccess)
{
    Console.WriteLine($"Saved to: {download.Value}");
}
```

**Save location behavior**

* If `saveTo` is **a directory**, the SDK saves as `<directory>/<UID>`
* If `saveTo` is **a full file path with an extension**, it saves exactly there
* If `saveTo` is not provided, default is: `~/Downloads/Walacor/<UID>`

### List files

```csharp
var list = await wal.FileRequestsService.ListFilesAsync(pageSize: 50, pageNo: 0);
if (list.IsSuccess)
{
    Console.WriteLine($"Files: {list.Value.Count}");
}
```

---

## Error handling (`Result<T>`)

Most SDK methods return a `Result<T>` object:

* `IsSuccess` indicates success
* `Value` contains the response data
* `Error` contains a structured error (validation/auth/server/etc.)
* `StatusCode`, `CorrelationId`, and sometimes `DurationMs` are included for debugging

Typical pattern:

```csharp
var res = await wal.SchemaService.GetVersionsAsync();
if (!res.IsSuccess)
{
    Console.WriteLine($"HTTP: {res.StatusCode}");
    Console.WriteLine($"Correlation: {res.CorrelationId}");
    Console.WriteLine($"Error: {res.Error?.Message}");
}
```

---

## Contributing

PRs are welcome. For larger changes, please open an issue first so we can agree on direction.

---

## License

Apache 2.0 — see `LICENSE`.
