import axios from 'axios';

// Tạo axios instance
const AxiosHelper = axios.create(
{
    baseURL: `${import.meta.env.VITE_BASE_URL}`,
    headers:
    {
        'Content-Type': 'application/json',
    },
});

// Interceptor để đính kèm token vào header
AxiosHelper.interceptors.request.use(async (config) => {
    const token = localStorage.getItem('Authentication-GOC-App-Token');
    if (token) {
        const userSession = JSON.parse(token);
        config.headers['Authorization'] = `Bearer ${userSession.accessToken}`;
    }
    return config;
}, (error) => {
    return Promise.reject(error);
});

export default AxiosHelper;