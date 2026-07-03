function isValidGUID(str:any) {
    // Regex to check valid
    // GUID  
    const regex = new RegExp(/^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$/);
 
    // if str 
    // is empty return false
    if (str == null) {
        return false;
    }
    if (typeof str == 'undefined') {
        return false;
    }
 
    // Return true if the str
    // matched the ReGex
    if (regex.test(str) == true) {
        return true;
    }
    else {
        return false;
    }
}
 export default isValidGUID