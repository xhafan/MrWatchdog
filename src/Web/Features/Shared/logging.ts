import { LogConstants } from "./Generated/LogConstants";
import StackTrace from "stacktrace-js";

export async function logError(
    error: string | Error | unknown,
    extraInfo: Record<string, unknown> = {},
    logErrorToConsole: boolean = false,
    showAlertDialog: boolean = false
): Promise<void> {
    if (logErrorToConsole) {
        console.error(error, extraInfo);
    }
    if (showAlertDialog) {
        bootbox.alert({
            title: '<i class="fa-solid fa-triangle-exclamation text-danger"></i> Error',
            message: `${error}`,
            centerVertical: true
        });
    }

    let errorMessage : string;
    if (error instanceof Error) {
        const tsStack = await GetTsStack(error);

        const errorInfo = {
            message: error.message,
            name: error.name,
            ...extraInfo,
            stack: tsStack,
        };
        errorMessage = `JS error: ${JSON.stringify(errorInfo)}`;
    }
    else {
        const errorInfo = {
            value: error,
            ...extraInfo
        };

        errorMessage = `JS error: ${JSON.stringify(errorInfo)}`;
    }

    const safeMessage = errorMessage.length > LogConstants.maxLogMessageLength
        ? errorMessage.slice(0, LogConstants.maxLogMessageLength) + "…[truncated]"
        : errorMessage;

    try {
        await fetch("/api/Logs/LogError", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(safeMessage)
        });
    } catch {
        // ignore errors
    }
}

async function GetTsStack(error: Error) : Promise<string | undefined> {
    try {
        if (!error.stack) return undefined;

        const stackframes = await StackTrace.fromError(error);

        // normalize fileName to remove host/URL and keep only /Features/... or /node_modules/... or /lib/...
        stackframes.forEach(sf => {
            if (sf.fileName) {
                const match = sf.fileName.match(/(\/Features\/.*|\/node_modules\/.*|\/lib\/.*)/);
                if (match) sf.fileName = match[1];
            }
        });

        return stackframes.map(sf => sf.toString()).join("\n");
    } catch (err) {
        console.error("Failed to map stack trace:", err);
    }
}