// tailwind.config.js
/** @type {import('tailwindcss').Config} */
export default {
  // This is the most IMPORTANT part: list ALL files that contain Tailwind classes
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}", // Scan all files in src/
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
