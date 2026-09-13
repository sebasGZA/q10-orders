import { ordersApi } from "../../api/orders.api";
import type { Order } from "../types/order.interface";

export const getOrdersAction = async (): Promise<Order[]> => {
    const res = await ordersApi(`/orders`);
    if (!res.ok) {
        throw new Error(`Error getting the orders (${res.status})`);
    }
    return (await res.json()) as Order[];
}