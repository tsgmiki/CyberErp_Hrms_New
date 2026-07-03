export default function errorMessageParser(error?: any) {
  let message = "";
  if (typeof error == "string" && error != "") {
    message = error;
    return message;
  }

  if (error && typeof error.response != "undefined") {
    const errorsData = error?.response?.data;
    
    // Handle ASP.NET Core validation error format
    if (errorsData?.errors && typeof errorsData.errors === "object") {
      const errors = errorsData.errors;
      const keys = Object.keys(errors);
      let messageStr = "";
      for (let i = 0; i < keys.length; i++) {
        const key = keys[i];
        const errorMessages = errors[key];
        if (Array.isArray(errorMessages)) {
          // Format field name from JSON path (e.g., "$.documentUrl" -> "documentUrl")
          const fieldName = key.startsWith("$.") ? key.slice(2) : key;
          for (const errorMsg of errorMessages) {
            messageStr += `${fieldName}: ${errorMsg}<br/>`;
          }
        } else {
          messageStr += `${i + 1}. ${errorMessages}<br/>`;
        }
      }
      message = messageStr;
    } else if (errorsData?.detail) {
      message = errorsData.detail;
    } else if (errorsData?.title) {
      message = errorsData.title;
    } else {
      const errors = errorsData?.errors;
      if (errors) {
        const keys = Object.keys(errors);
        let messageStr = "";
        for (let i = 0; i < keys.length; i++) {
          messageStr =
            messageStr + (i + 1).toString() + " " + errors[keys[i]] + "<br/>";
        }
        message = messageStr;
      }
    }
  } else if (error && typeof error.message != "undefined") {
    message = error.message;
  } else if (error && error.errors && typeof error.errors === "object") {
    // Handle ASP.NET Core validation error format directly (no response wrapper)
    const errors = error.errors;
    const keys = Object.keys(errors);
    let messageStr = "";
    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];
      const errorMessages = errors[key];
      if (Array.isArray(errorMessages)) {
        // Format field name from JSON path (e.g., "$.documentUrl" -> "documentUrl")
        const fieldName = key.startsWith("$.") ? key.slice(2) : key;
        for (const errorMsg of errorMessages) {
          messageStr += `${fieldName}: ${errorMsg}<br/>`;
        }
      } else {
        messageStr += `${i + 1}. ${errorMessages}<br/>`;
      }
    }
    message = messageStr;
  } else if (error && error.title) {
    // Handle ASP.NET Core error with title directly
    message = error.title;
  } else if (error) {
    const keys = Object.keys(error);
    let messageStr = "";
    for (let i = 0; i < keys.length; i++) {
      messageStr =
        messageStr + (i + 1).toString() + " " + error[keys[i]] + "<br/>";
    }
    message = messageStr;
  }
  return message;
}
