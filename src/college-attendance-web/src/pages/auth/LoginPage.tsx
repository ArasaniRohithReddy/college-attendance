import { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: object) => void;
          renderButton: (el: HTMLElement, config: object) => void;
        };
      };
    };
  }
}

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? '';

export default function LoginPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const callbackRef = useRef(login);
  const initializedRef = useRef(false);

  useEffect(() => {
    callbackRef.current = login;
  }, [login]);

  useEffect(() => {
    if (isAuthenticated) navigate('/dashboard', { replace: true });
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    if (initializedRef.current) return;

    const handleCredentialResponse = async (response: { credential: string }) => {
      try {
        await callbackRef.current(response.credential);
        navigate('/dashboard', { replace: true });
      } catch {
        alert('Login failed. Please try again.');
      }
    };

    const initGoogle = () => {
      if (initializedRef.current) return;
      initializedRef.current = true;
      window.google?.accounts.id.initialize({
        client_id: GOOGLE_CLIENT_ID,
        callback: handleCredentialResponse,
      });
      const btn = document.getElementById('google-signin-btn');
      if (btn) {
        window.google?.accounts.id.renderButton(btn, {
          theme: 'outline',
          size: 'large',
          width: 320,
          text: 'signin_with',
        });
      }
    };

    if (window.google?.accounts) {
      initGoogle();
    } else {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.onload = initGoogle;
      document.head.appendChild(script);
    }
  }, [navigate]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-indigo-50 via-white to-indigo-50">
      <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-indigo-600 text-2xl font-bold text-white">
            A
          </div>
          <h1 className="text-2xl font-bold text-gray-900">AttendEase</h1>
          <p className="mt-2 text-gray-500">College Attendance Management System</p>
        </div>
        <div className="flex justify-center">
          <div id="google-signin-btn" />
        </div>
        <p className="mt-6 text-center text-xs text-gray-400">
          Sign in with your college Google account
        </p>
      </div>
    </div>
  );
}
