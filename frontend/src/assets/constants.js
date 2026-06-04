/**
 * Stałe strony logowania / rejestracji (inline style z mockupu).
 * Paleta odpowiada zmiennym :root w styles/01-foundation.css.
 */

export const T = {
  bg: '#FAF7F2',
  white: '#FFFFFF',
  green: '#2D6A4F',
  greenHov: '#245A42',
  greenLoading: '#7BAF9A',
  gold: '#D4A843',
  blue: '#2C5F8A',
  t1: '#2C1810',
  t2: '#7A6652',
  t3: '#B5A090',
  border: '#E8DDD4',
  tagBg: '#F0E8DF',
  danger: '#C0392B',
  success: '#27AE60',
  successBg: '#EBF7F0',
  errorBg: '#FBEDEC',
  warning: '#E67E22',
};

/** Wspólne obiekty :style dla formularzy auth. */
export const AUTH_STYLES = {
  successIconCircle: {
    width: '64px',
    height: '64px',
    borderRadius: '50%',
    background: T.successBg,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    margin: '0 auto 20px',
    animation: 'successPulse 0.6s ease 0.2s',
  },
  successScreen: {
    textAlign: 'center',
    padding: '20px 0',
    animation: 'fadeScaleIn 0.4s ease both',
  },
  errorAlert: {
    background: T.errorBg,
    color: T.danger,
    fontSize: '13px',
    padding: '10px 12px',
    borderRadius: '8px',
    border: `1px solid ${T.danger}33`,
  },
  submitButton(loading, hov) {
    return {
      width: '100%',
      height: '46px',
      background: loading ? T.greenLoading : (hov ? T.greenHov : T.green),
      color: 'white',
      border: 'none',
      borderRadius: '8px',
      fontSize: '15px',
      fontWeight: 600,
      fontFamily: "'DM Sans', sans-serif",
      cursor: loading ? 'not-allowed' : 'pointer',
      transition: 'background 0.18s ease',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      gap: '8px',
      letterSpacing: '0.2px',
    };
  },
  submitSpinner: {
    width: '16px',
    height: '16px',
    flexShrink: 0,
    border: '2px solid rgba(255,255,255,0.3)',
    borderTopColor: 'white',
    borderRadius: '50%',
    display: 'inline-block',
    animation: 'spin 0.7s linear infinite',
  },
  heading: {
    fontFamily: "'Playfair Display', serif",
    fontSize: '28px',
    fontWeight: 700,
    color: T.t1,
    marginBottom: '6px',
    lineHeight: 1.2,
  },
  subheading: {
    color: T.t2,
    fontSize: '14px',
  },
  successTitle: {
    fontFamily: "'Playfair Display', serif",
    fontSize: '22px',
    fontWeight: 700,
    color: T.t1,
    marginBottom: '8px',
  },
};

export const P = {
  book: `<path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>`,
  mail: `<rect x="2" y="4" width="20" height="16" rx="2"/><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/>`,
  lock: `<rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>`,
  eye: `<path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/>`,
  eyeOff: `<path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><line x1="2" y1="2" x2="22" y2="22"/>`,
  at: `<circle cx="12" cy="12" r="4"/><path d="M16 8v5a3 3 0 0 0 6 0v-1a10 10 0 1 0-3.92 7.94"/>`,
  pin: `<path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/>`,
  check: `<polyline points="20,6 9,17 4,12"/>`,
  xmark: `<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>`,
  users: `<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>`,
  repeat: `<polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/>`,
  star: `<polygon points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26"/>`,
};

/** Kolory odznak na dekoracyjnym panelu auth (lewa kolumna). */
export const AUTH_DEMO_BOOK_BADGE = {
  loan: { bg: T.green, text: 'white' },
  swap: { bg: T.gold, text: T.t1 },
  sale: { bg: T.blue, text: 'white' },
};
