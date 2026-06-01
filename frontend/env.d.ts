/// <reference types="vite/client" />

// Pozwala TypeScriptowi rozumieć importy komponentów .vue jako moduły.
declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<Record<string, unknown>, Record<string, unknown>, unknown>
  export default component
}
