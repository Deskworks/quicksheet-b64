# quicksheet-b64

Base64 encode/decode on your [QuickSheet](https://github.com/cemheren/QuickSheet) spreadsheet — decode JWTs, API tokens, config blobs, or encode text for embedding.

## Install

Type into any QuickSheet cell:

```
ext: github:Deskworks/quicksheet-b64
```

## Usage

```
b64: SGVsbG8gV29ybGQ=          # Auto-detects base64 → decodes to "Hello World"
b64: Hello World                # Auto-detects plain text → encodes to base64
b64: decode SGVsbG8=            # Explicit decode
b64: encode Hello World         # Explicit encode
b64: dec eyJhbGciOiJIUzI1NiJ9  # Decode a JWT header
```

### Auto-detect

When no `encode`/`decode` prefix is given, the extension auto-detects:
- If the input looks like valid base64 (only `A-Za-z0-9+/=` chars and decodes successfully), it **decodes** it
- Otherwise, it **encodes** the input as base64

### Explicit modes

| Prefix | Aliases | Action |
|--------|---------|--------|
| `encode` | `enc`, `e` | Force encode to base64 |
| `decode` | `dec`, `d` | Force decode from base64 |

## Output

- Multi-line decoded text is split across rows
- Long encoded output is wrapped at 76 characters (MIME standard)
- Byte count shown for both encode and decode
- Max 20 rows of output

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [QuickSheet](https://github.com/cemheren/QuickSheet)

## License

MIT
