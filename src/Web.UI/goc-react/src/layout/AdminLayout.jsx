import React from 'react';

const AdminLayout = ({ children }) => {
    return (
        <div className="admin-layout">
            {/* Add your layout styling and structure for admin-side pages */}
            <header>Admin Header</header>
            <main>{children}</main>
            <footer>Admin Footer</footer>
        </div>
    );
};

export default AdminLayout;
