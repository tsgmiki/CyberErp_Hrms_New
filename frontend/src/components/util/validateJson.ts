function isValidJson(text: any) {
  try {
    JSON.parse(text);
    return true;
  } catch (err) {
    return false;
  }
}
export default isValidJson;
