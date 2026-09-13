import { useState } from 'react';
import './index.css';
import { OrderForm } from './orders/components/OrderForm';
import { OrderList } from './orders/components/OrderList';

export default function App() {
  const [refreshSignal, setRefreshSignal] = useState(0);

  return (
    <div className="app">
      <header>
        <h1>Orders</h1>
      </header>
      <main>
        <OrderForm onCreated={() => setRefreshSignal((v) => v + 1)} />
        <OrderList refreshSignal={refreshSignal} />
      </main>
    </div>
  )
}
