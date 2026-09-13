import { ordersApi } from "../../api/orders.api";
import type { PaginationResponse } from "../../types/pagination-response.interface";
import type { Order } from "../types/order.interface";

export const getOrdersAction = async (page = 1, pageSize: number = 10): Promise<PaginationResponse<Order>> => {
    const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize)
    });
    const res = await ordersApi(`/orders?${params}`);
    if (!res.ok) {
        throw new Error(`Error getting the orders (${res.status})`);
    }
    return (await res.json());
}