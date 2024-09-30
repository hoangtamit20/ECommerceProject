import { Routes, Route } from 'react-router-dom';
import ProtectedRoute from './services/auth/base/ProtectedRoute';
import routes from './services/auth/base/Route';
import AuthLayOut from './layout/AuthLayOut';
import ClientLayOut from './layout/ClientLayOut';
import AdminLayout from './layout/AdminLayout';
import RouteListener from './services/auth/base/RouteListener';

const App = () => {
    return (
        <>
            {/* Place RouteListener outside of Routes */}
            {/* <RouteListener />  */}
            
            <Routes>
                {routes.map((route, index) => {
                    if (route.public) {
                        // Use AuthLayOut for public routes (auth routes)
                        return (
                            <Route 
                                key={index} 
                                path={route.path} 
                                element={
                                    <AuthLayOut>
                                        <route.component />
                                    </AuthLayOut>
                                } 
                            />
                        );
                    } else if (route.roles?.includes('Admin')) {
                        // Use AdminLayout for routes with the Admin role
                        return (
                            <Route
                                key={index}
                                path={route.path}
                                element={
                                    <ProtectedRoute roles={route.roles}>
                                        <AdminLayout>
                                            <route.component />
                                        </AdminLayout>
                                    </ProtectedRoute>
                                }
                            />
                        );
                    } else {
                        // Use ClientLayOut for protected client routes or special public routes like homepage
                        return (
                            <Route
                                key={index}
                                path={route.path}
                                element={
                                    <ProtectedRoute roles={route.roles}>
                                        <ClientLayOut>
                                            <route.component />
                                        </ClientLayOut>
                                    </ProtectedRoute>
                                }
                            />
                        );
                    }
                })}
            </Routes>
        </>
    );
};

export default App;