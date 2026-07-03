const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function getHealthCkeck() {
  try {
    const response = await fetch(`${API_BASE_URL}/Health/live`, {
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to fetch bank");
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching bank:", error);
    throw error;
  }
}
