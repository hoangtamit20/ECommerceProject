import { createRoot } from 'react-dom/client';
import App from './App.jsx';
import { ToastContainer } from 'react-toastify';
import { BrowserRouter } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import 'react-toastify/ReactToastify.css';

createRoot(document.getElementById('root')).render(
  <BrowserRouter>
    <>
      <App />
      <ToastContainer />
    </>
  </BrowserRouter>
);
