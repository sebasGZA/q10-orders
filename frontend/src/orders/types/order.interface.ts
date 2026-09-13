import type { OrderStatus } from "./order-status.type";

export interface Order {
  id: string;
  clientName: string;
  sku: string;
  quantity: number;
  status: OrderStatus;
  createdAt: string;
}
