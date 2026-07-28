/**
 * Tauri Hardware IPC Bridge (Optional Desktop Companion)
 * When running in Tauri Rust app shell, connects directly to native OS APIs.
 */
(function() {
  if (window.__TAURI__) {
    console.log("⚡ LiteOverlay Native Tauri Host detected.");
    
    // Periodically query Tauri native Rust backend for CPU/GPU hardware sensors
    async function fetchNativeStats() {
      try {
        const stats = await window.__TAURI__.invoke('get_system_stats');
        window.__TAURI_SYSTEM_INFO__ = stats;
      } catch (err) {
        // Fallback
      }
    }
    
    fetchNativeStats();
    setInterval(fetchNativeStats, 1000);
  }
})();
