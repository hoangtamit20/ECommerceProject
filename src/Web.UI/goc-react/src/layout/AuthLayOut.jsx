import React from 'react';
import './css/authlayout.css'

const AuthLayOut = ({ children }) => {
    return (
        <div className="auth-layout">
            {/* Add your layout styling and structure for auth pages */}
            {children}
        </div>
    );
};

export default AuthLayOut;