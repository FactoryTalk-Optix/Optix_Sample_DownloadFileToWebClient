# Download Files To Web Client

`FilesLister` is a C# application designed to list files in a specified directory and serve them over HTTP. This application is built using the HttpListener framework.

## Configuration

Open the project and navigate to the `NetLogic` folder, here the `FilesLister` Runtime NetLogic can be configured with:

1. **BaseDirectory**: The base directory from which files will be listed and served.
2. **ServerAddress**: The address on which the HTTP server will listen.
3. **ServerPort**: The port on which the HTTP server will listen.

## Usage

- Execute the Runtime and navigate to `http://127.0.0.1:8081` using the WebBrowser to see the list of files in the configured folder (and subfolders)
- Click on any file to initiate the download operation

## NetLogic description

1. **Start the HTTP Listener**: The `Start` method initializes and starts the HTTP listener. It reads the configuration variables, validates them, and starts listening for incoming HTTP requests.

2. **Stop the HTTP Listener**: The `Stop` method stops the HTTP listener.

3. **Handle HTTP Requests**: The `OnRequest` method handles incoming HTTP requests. It supports two types of requests:
   - **Root URL (`/`)**: Lists all files in the specified directory.
   - **File Download (`/download/{file}`)**: Serves the requested file for download.

## Important note

- **Security**: This application does not support SSL/TLS. It is highly recommended to use this application behind a reverse proxy that provides SSL/TLS support to ensure secure communication.
- **Validation**: Ensure that the `ServerAddress` and `ServerPort` are correctly configured to avoid potential security risks.
- **Access Control**: This application does not implement any form of access control. It is recommended to restrict access to trusted users only.
- **Directory Traversal**: The application attempts to prevent directory traversal attacks by serving files only from the specified base directory. However, additional validation and security measures should be implemented as needed.

## Known issues

- The web server cannot be embedded using the FactoryTalk Optix `WebBrowser` object in the `NativePresentationEngine` due to CORS limitations

## Cloning the repository

There are multiple ways to download this project, here is the recommended one

### Clone repository with FactoryTalk Optix

1. Click on the green `CODE` button in the top right corner
2. Select `HTTPS` and copy the provided URL
3. Open FT Optix IDE
4. Click on `Open` and select the `Remote` tab
5. Paste the URL from step 2
6. Click `Open` button in bottom right corner to start cloning process

## Disclaimer

Rockwell Automation maintains these repositories as a convenience to you and other users. Although Rockwell Automation reserves the right at any time and for any reason to refuse access to edit or remove content from this Repository, you acknowledge and agree to accept sole responsibility and liability for any Repository content posted, transmitted, downloaded, or used by you. Rockwell Automation has no obligation to monitor or update Repository content

The examples provided are to be used as a reference for building your own application and should not be used in production as-is. It is recommended to adapt the example for the purpose, observing the highest safety standards.
