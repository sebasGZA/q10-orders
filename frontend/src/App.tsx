import './index.css';
import { OrderPage } from './orders/pages/OrderPage';

export default function App() {

  return (
    <div className="app">
      <header>
        <h1>Orders</h1>
      </header>
      <main>
        <OrderPage />
      </main>
    </div>
  )
}
