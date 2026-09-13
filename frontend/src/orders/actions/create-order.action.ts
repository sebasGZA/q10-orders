import { ordersApi } from "../../api/orders.api";
import type { ApiError } from "../../types/api-error.interface";
import type { CreateOrder } from "../types/create-order.interface";
import type { Order } from "../types/order.interface";

export const createOrderAction = async (payload: CreateOrder): Promise<Order> => {
    const res = await ordersApi(`/orders`, {
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