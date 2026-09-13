import type { CreateOrder } from "../orders/types/create-order.interface";
import type { Order } from "../orders/types/order.interface";
import type { ApiError } from "../types/api-error.interface";

const API_URL = import.meta.env.VITE_API_URL ?? "";

export async function createOrder(payload: CreateOrder): Promise<Order> {
  const res = await fetch(`${API_URL}/orders`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!res.ok) {
    const body = (await res.json().catch(() => null)) as ApiError | null;
    throw new Error(body?.error ?? `Error creating the order (${res.status})`);
  }

  return (await res.json()) as Order;
}

export async function fetchOrders(): Promise<Order[]> {
  const res = await fetch(`${API_URL}/orders`);
  if (!res.ok) {
    throw new Error(`Error getting the orders (${res.status})`);
  }
  return (await res.json()) as Order[];
}
