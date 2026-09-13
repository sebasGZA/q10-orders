import { useState } from "react";
import { OrderForm } from "../components/OrderForm";
import { OrderList } from "../components/OrderList";

export const OrderPage = () => {
    const [refreshSignal, setRefreshSignal] = useState(0)
    return (
        <>
            <OrderForm onCreated={() => setRefreshSignal((v) => v + 1)} />
            <OrderList refreshSignal={refreshSignal} />
        </>
    )
}