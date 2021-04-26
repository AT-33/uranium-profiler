use std::time::Instant;
use chrono::{Datelike, Timelike};
use std::fs::File;
use std::io::Write;

#[cfg(test)]
mod tests;

pub static mut START_TIME: Option<Instant> = None;
pub static mut EVENTS: Vec<(u128, &'static str, EventType)> = Vec::new();

#[derive(Debug, Copy, Clone, PartialEq)]
pub enum EventType {
    Begin,
    End,
}

#[inline]
pub fn profiler_event(name: &'static str, event_type: EventType) {
    unsafe {
        let timestamp = START_TIME
            .expect("Profiler must be initialized with initialize_profiler()")
            .elapsed()
            .as_micros();
        EVENTS.push((timestamp, name, event_type));
    }
}

pub fn initialize_profiler() {
    unsafe {
        START_TIME = Some(Instant::now());
    }
}

pub fn save_current_session() {
    let now = chrono::Local::now();
    let date_time = format!("{:02} {:02} {} - {:02}.{:02}.{:02}",
                            now.day(), now.month(), now.year(),
                            now.hour(), now.minute(), now.second());
    let file_name = format!("uranium session [{}].ups", date_time);

    let mut file = File::create(file_name)
        .expect("An error creating new file");
    for event in unsafe { &EVENTS } {
        let event_type = match event.2 {
            EventType::Begin => 'B',
            EventType::End => 'E',
        };
        let line = format!("{}{}{}\n", event_type, event.0, event.1);
        file.write_all(line.as_bytes())
            .expect("Failed to write to file");
    }
}
