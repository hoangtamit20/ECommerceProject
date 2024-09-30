import { createContext, useContext, useEffect, useState } from 'react';
import { ApiClient } from './APIClient';

// Tạo context
const AuthContext = createContext();

// Tạo AuthProvider để quản lý trạng thái đăng nhập
export const AuthProvider = ({ children }) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [userRoles, setUserRoles] = useState([]);

    useEffect(() => {
        const checkAuth = async () => {
            const isAuth = await ApiClient.isAuthenticated();
            if (isAuth) {
                setIsAuthenticated(true);
                const roles = await ApiClient.getUserRoles();  // Lấy user roles
                setUserRoles(roles);
            } else {
                const isRefreshed = await ApiClient.refreshToken();
                setIsAuthenticated(isRefreshed);
                if (isRefreshed) {
                    const roles = await ApiClient.getUserRoles();  // Lấy user roles sau khi refresh token
                    setUserRoles(roles);
                }
            }
        };
        checkAuth();
    }, []);

    return (
        <AuthContext.Provider value={{ isAuthenticated, userRoles, setIsAuthenticated }}>
            {children}
        </AuthContext.Provider>
    );
};

// Hook để sử dụng AuthContext
export const useAuth = () => useContext(AuthContext);