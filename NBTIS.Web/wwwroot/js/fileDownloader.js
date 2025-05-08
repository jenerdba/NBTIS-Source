// wwwroot/js/fileDownloader.js

export function downloadFile(fileName, mimeType, data) {
    // Create a Blob from the data
    const blob = new Blob([data], { type: mimeType });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    // Programmatically click the anchor to trigger the download
    a.click();
    // Clean up: remove the anchor and revoke the object URL
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}
