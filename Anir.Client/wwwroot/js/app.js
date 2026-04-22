// wwwroot/js/app.js

// 1. Descargar archivos (Base64)
window.downloadFile = (fileName, contentType, base64Data) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64Data}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// 2. Abrir PDF desde una URL en nueva pestaña (Sin descargar)
window.openPdfInNewTab = function(url) {
    const newWindow = window.open("", "_blank");
    if (newWindow) {
        newWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head><title>Visor PDF</title></head>
            <body style="margin:0; overflow:hidden;">
                <iframe src="${url}" style="width:100%; height:100vh; border:none;"></iframe>
            </body>
            </html>
        `);
        newWindow.document.close();
    }
};

// 3. Abrir PDF desde un Array de Bytes (Para los reportes generados por QuestPDF)
window.openPdfBlob = (bytes, fileName) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: "application/pdf" });
    const blobUrl = URL.createObjectURL(blob);
    
    const newWindow = window.open("", "_blank");
    if (newWindow) {
        newWindow.document.write(`
            <!DOCTYPE html>
            <html>
            <head><title>${fileName || 'Visor PDF'}</title></head>
            <body style="margin:0; overflow:hidden;">
                <iframe src="${blobUrl}" style="width:100%; height:100vh; border:none;"></iframe>
            </body>
            </html>
        `);
        newWindow.document.close();
    }
    
    setTimeout(() => window.URL.revokeObjectURL(blobUrl), 60000);
};

// 4. Descargar Excel o cualquier archivo desde Bytes
window.downloadBlob = (bytes, fileName, contentType) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: contentType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || "archivo.xlsx";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    setTimeout(() => window.URL.revokeObjectURL(url), 1000);
};