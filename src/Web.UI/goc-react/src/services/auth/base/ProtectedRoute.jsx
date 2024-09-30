import React, { useState, useEffect } from 'react';
import { Route, Navigate } from 'react-router-dom'; // Use Navigate instead of Redirect
import { useAuth } from './AuthProvider';
import { ApiClient } from './APIClient';
import { toast } from 'react-toastify';

const ProtectedRoute = ({ component: Component, roles, ...rest }) => {
    const { isAuthenticated, userRoles } = useAuth();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (isAuthenticated) {
            const checkRoles = async () => {
                const roles = await ApiClient.getUserRoles();
                if (!roles || roles.length === 0) {
                    toast.error("Không tìm thấy roles nào cho người dùng.");
                }
                setLoading(false);
            };

            checkRoles();
        } else {
            setLoading(false); // Stop loading if not authenticated
        }
    }, [isAuthenticated]);

    if (loading) {
        return <div>Loading...</div>; // Or a professional loading component
    }

    return (
        <Route
            {...rest}
            element={
                isAuthenticated ? (
                    roles && !roles.some(role => userRoles.includes(role)) ? (
                        <>
                            {toast.error("Bạn không có quyền truy cập trang này.")}
                            <Navigate to="/khong-co-quyen" />
                        </>
                    ) : (
                        <Component />
                    )
                ) : (
                    <Navigate to="/login" />
                )
            }
        />
    );
};

export default ProtectedRoute;