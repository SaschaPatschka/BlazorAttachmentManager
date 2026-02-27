// JavaScript Interop for FileUploadManager

// Export functions for ES6 module import
export function initializePasteHandler(dotNetHelper, elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.warn('Paste area element not found:', elementId);
        return;
    }

    // Make element focusable if not already
    if (!element.hasAttribute('tabindex')) {
        element.setAttribute('tabindex', '0');
    }

    // Focus the element to ensure it receives paste events
    element.addEventListener('click', () => element.focus());

    // Handle paste event
    const pasteHandler = async (e) => {
        const items = e.clipboardData?.items;
        if (!items) return;

        for (let i = 0; i < items.length; i++) {
            if (items[i].type.indexOf('image') !== -1) {
                e.preventDefault();
                const blob = items[i].getAsFile();

                if (blob) {
                    const reader = new FileReader();
                    reader.onload = async function(event) {
                        const base64 = event.target.result;
                        try {
                            await dotNetHelper.invokeMethodAsync('HandleClipboardImage', base64, blob.type);
                        } catch (error) {
                            console.error('Error calling HandleClipboardImage:', error);
                        }
                    };
                    reader.readAsDataURL(blob);
                }
                break;
            }
        }
    };

    element.addEventListener('paste', pasteHandler);

    // Also add global paste handler for better UX
    document.addEventListener('paste', pasteHandler);

    // Store for cleanup if needed
    element._pasteHandler = pasteHandler;
}

// Read clipboard programmatically when button is clicked
export async function readClipboard(dotNetHelper) {
    console.log('readClipboard called', dotNetHelper);

    try {
        // Check if Clipboard API is available
        if (!navigator.clipboard || !navigator.clipboard.read) {
            console.warn('Clipboard API not supported');
            alert('Zwischenablage-API wird in diesem Browser nicht unterstützt. Bitte verwenden Sie Strg+V stattdessen.');
            return;
        }

        console.log('Reading clipboard...');
        // Request clipboard permission and read
        const clipboardItems = await navigator.clipboard.read();
        console.log('Clipboard items:', clipboardItems);

        for (const clipboardItem of clipboardItems) {
            console.log('Processing clipboard item, types:', clipboardItem.types);

            for (const type of clipboardItem.types) {
                if (type.startsWith('image/')) {
                    console.log('Found image type:', type);
                    const blob = await clipboardItem.getType(type);
                    console.log('Blob size:', blob.size);

                    const reader = new FileReader();
                    reader.onload = async function(event) {
                        const base64 = event.target.result;
                        console.log('Base64 data length:', base64.length);

                        try {
                            console.log('Calling HandleClipboardImage...');
                            await dotNetHelper.invokeMethodAsync('HandleClipboardImage', base64, type);
                            console.log('HandleClipboardImage completed');
                        } catch (error) {
                            console.error('Error calling HandleClipboardImage:', error);
                            alert('Fehler beim Verarbeiten des Bildes: ' + error.message);
                        }
                    };

                    reader.onerror = function(error) {
                        console.error('FileReader error:', error);
                        alert('Fehler beim Lesen des Bildes.');
                    };

                    reader.readAsDataURL(blob);
                    return; // Only process first image
                }
            }
        }

        console.log('No image found in clipboard');
        alert('Kein Bild in der Zwischenablage gefunden.');
    } catch (error) {
        console.error('Error reading clipboard:', error);
        alert('Fehler beim Lesen der Zwischenablage: ' + error.message + '\n\nBitte verwenden Sie stattdessen Strg+V.');
    }
}


    // Download file
    export function downloadFile(fileName, contentType, base64Data) {
        const link = document.createElement('a');
        link.href = `data:${contentType};base64,${base64Data}`;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    // Enhanced drag and drop support
    export function initializeDragDrop(dropZoneElement, inputFileElement) {
        if (!dropZoneElement || !inputFileElement) return;

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZoneElement.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        // Handle drop
        dropZoneElement.addEventListener('drop', async function(e) {
            const dt = e.dataTransfer;
            const files = dt.files;

            // Trigger the InputFile component with the dropped files
            if (files.length > 0) {
                const dataTransfer = new DataTransfer();
                for (let i = 0; i < files.length; i++) {
                    dataTransfer.items.add(files[i]);
                }
                inputFileElement.files = dataTransfer.files;

                // Trigger change event
                const event = new Event('change', { bubbles: true });
                inputFileElement.dispatchEvent(event);
            }
        }, false);
    }

    // Compress image to meet size requirements
    export async function compressImage(base64Data, maxSizeBytes, qualityLevels, maxDimension) {
        console.log('compressImage called', { 
            originalSize: base64Data.length, 
            maxSizeBytes, 
            qualityLevels, 
            maxDimension 
        });

        try {
            // Create image element
            const img = new Image();

            // Load image
            await new Promise((resolve, reject) => {
                img.onload = resolve;
                img.onerror = reject;
                img.src = base64Data;
            });

            console.log('Image loaded', { width: img.width, height: img.height });

            // Calculate new dimensions if needed
            let width = img.width;
            let height = img.height;

            if (width > maxDimension || height > maxDimension) {
                if (width > height) {
                    height = Math.round((height * maxDimension) / width);
                    width = maxDimension;
                } else {
                    width = Math.round((width * maxDimension) / height);
                    height = maxDimension;
                }
                console.log('Resizing to', { width, height });
            }

            // Try each quality level
            for (let i = 0; i < qualityLevels.length; i++) {
                const quality = qualityLevels[i];
                console.log(`Trying compression quality: ${quality}`);

                // Create canvas
                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;

                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);

                // Convert to blob with quality
                const blob = await new Promise(resolve => {
                    canvas.toBlob(resolve, 'image/jpeg', quality);
                });

                // Convert blob to base64
                const reader = new FileReader();
                const compressedBase64 = await new Promise(resolve => {
                    reader.onloadend = () => resolve(reader.result);
                    reader.readAsDataURL(blob);
                });

                const compressedSize = compressedBase64.length;
                console.log(`Compressed size at quality ${quality}: ${compressedSize} bytes`);

                // Check if size is acceptable
                if (compressedSize <= maxSizeBytes || i === qualityLevels.length - 1) {
                    console.log('Compression successful', { 
                        originalSize: base64Data.length, 
                        compressedSize, 
                        quality,
                        reduction: ((1 - compressedSize / base64Data.length) * 100).toFixed(1) + '%'
                    });

                    return {
                        success: true,
                        data: compressedBase64,
                        originalSize: base64Data.length,
                        compressedSize: compressedSize,
                        quality: quality
                    };
                }
            }

            // If we get here, compression failed
            return {
                success: false,
                message: 'Could not compress image to required size',
                originalSize: base64Data.length
            };

        } catch (error) {
            console.error('Error compressing image:', error);
            return {
                success: false,
                message: error.message,
                originalSize: base64Data.length
            };
        }
    }
