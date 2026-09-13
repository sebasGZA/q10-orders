import { useEffect, useState } from "react";
import type { Order } from "../types/order.interface";
import { getOrdersAction } from "../actions/get-orders.action";

const POLL_INTERVAL_MS = 3000;
const PAGE_SIZE = 10;

function badgeClass(status: Order["status"]) {
  switch (status) {
    case "Confirmed":
      return "badge badge-confirmed";
    case "Rejected":
      return "badge badge-rejected";
    default:
      return "badge badge-pending";
  }
}

interface Props {
  refreshSignal: number;
}

export const OrderList = ({ refreshSignal }: Props) => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [error, setError] = useState<string | null>(null);

  async function load(targetPage: number) {
    try {
      const data = await getOrdersAction(targetPage, PAGE_SIZE);
      setOrders(data.items);
      setTotalPages(Math.max(data.totalPages, 1));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Orders cannot be loaded.");
    }
  }

  useEffect(() => { setPage(1); }, [refreshSignal]);
  useEffect(() => { load(page); }, [page, refreshSignal]);
  useEffect(() => {
    const interval = setInterval(() => load(page), POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [page]);

  return (
    <div className="order-list">
      <h2>Orders</h2>
      {error && <p className="error" role="alert">{error}</p>}

      <table>
        <thead>
          <tr>
            <th>Client</th>
            <th>SKU</th>
            <th>Quantity</th>
            <th>Status</th>
            <th>Created At</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((o) => (
            <tr key={o.id}>
              <td>{o.clientName}</td>
              <td>{o.sku}</td>
              <td>{o.quantity}</td>
              <td><span className={badgeClass(o.status)}>{o.status}</span></td>
              <td>{new Date(o.createdAt).toLocaleString()}</td>
            </tr>
          ))}
          {orders.length === 0 && (
            <tr>
              <td colSpan={5}>There are no orders.</td>
            </tr>
          )}
        </tbody>
      </table>

      <div className="pagination">
        <button onClick={() => setPage((p) => Math.max(p - 1, 1))} disabled={page <= 1}>Preview</button>
        <span>Page {page} of {totalPages}</span>
        <button onClick={() => setPage((p) => Math.min(p + 1, totalPages))} disabled={page >= totalPages}>Next</button>
      </div>
    </div>
  );
}
