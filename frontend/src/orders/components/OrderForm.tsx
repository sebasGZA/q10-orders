import { useState, type SubmitEvent } from "react";
import { createOrder } from "../../api/orders.api";

const SKUS = ["ABC-01", "ABC-02", "ABC-03"];

interface Props {
    onCreated: () => void;
}

export function OrderForm({ onCreated }: Props) {

    const [clientName, setClientName] = useState("");
    const [sku, setSku] = useState(SKUS[0]);
    const [quantity, setQuantity] = useState(1);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    async function handleSubmit(e: SubmitEvent<HTMLElement>) {
        e.preventDefault();
        setError(null);

        if (!clientName.trim()) {
            setError("The client name is required.");
            return;
        }
        if (quantity < 1) {
            setError("The min value allowed is 1.");
            return;
        }

        setLoading(true);
        try {
            await createOrder({ clientName: clientName.trim(), sku, quantity });
            setClientName("");
            setQuantity(1);
            onCreated();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Unexpected error creating order.");
        } finally {
            setLoading(false);
        }
    }

    return (
        <form onSubmit={handleSubmit} className="order-form">
            <h2>New order</h2>

            <label>
                Client
                <input
                    type="text"
                    value={clientName}
                    onChange={(e) => setClientName(e.target.value)}
                    placeholder="Client's name"
                />
            </label>

            <label>
                SKU
                <select value={sku} onChange={(e) => setSku(e.target.value)}>
                    {SKUS.map((s) => (
                        <option key={s} value={s}>
                            {s}
                        </option>
                    ))}
                </select>
            </label>

            <label>
                Quantity
                <input
                    type="number"
                    min={1}
                    value={quantity}
                    onChange={(e) => setQuantity(Number(e.target.value))}
                />
            </label>

            {error && <p className="error" role="alert">{error}</p>}

            <button type="submit" disabled={loading}>
                {loading ? "Creating..." : "Create order"}
            </button>
        </form>
    );
}
