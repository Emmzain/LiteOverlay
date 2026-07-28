// Prevents additional console window on Windows in release
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

#[derive(serde::Serialize)]
struct SystemStats {
    cpu_usage: f32,
    gpu_usage: f32,
    ram_usage_mb: u64,
}

#[tauri::command]
fn get_system_stats() -> SystemStats {
    SystemStats {
        cpu_usage: 18.5,
        gpu_usage: 32.0,
        ram_usage_mb: 1250,
    }
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![get_system_stats])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
