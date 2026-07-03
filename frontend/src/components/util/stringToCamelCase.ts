function toPascalCase(str: string): string {
  return str
    .trim()
    .split(/[^a-zA-Z0-9]+/) // split on non-alphanumeric
    .filter(Boolean)        // remove empty parts
    .map(
      word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
    )
    .join("");
}

export default toPascalCase;