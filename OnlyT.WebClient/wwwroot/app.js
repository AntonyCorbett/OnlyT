const API_BASE_URL = 'http://localhost:8096'; // Update with your OnlyT API port

// Track all timers and last update time for local updates
let allTimersData = null;
let lastUpdateTime = null;

// Get all timers
async function getAllTimers() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/v4/timers`);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json();
        console.log('All timers:', data);
        return data;
    } catch (error) {
        console.error('Error fetching timers:', error);
        throw error;
    }
}

// Get specific timer
async function getTimer(talkId) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/v4/timers/${talkId}`);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json();
        console.log(`Timer ${talkId}:`, data);
        return data;
    } catch (error) {
        console.error(`Error fetching timer ${talkId}:`, error);
        throw error;
    }
}

// Start timer
async function startTimer(talkId) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/v4/timers/${talkId}`, {
            method: 'POST'
        });
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json();
        console.log(`Started timer ${talkId}:`, data);
        return data;
    } catch (error) {
        console.error(`Error starting timer ${talkId}:`, error);
        throw error;
    }
}

// Stop timer
async function stopTimer(talkId) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/v4/timers/${talkId}`, {
            method: 'DELETE'
        });
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json();
        console.log(`Stopped timer ${talkId}:`, data);
        return data;
    } catch (error) {
        console.error(`Error stopping timer ${talkId}:`, error);
        throw error;
    }
}

// Change timer duration
async function changeDuration(talkId, deltaSeconds) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/v4/timers/${talkId}/duration`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ deltaSeconds })
        });
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json();
        console.log(`Changed duration for timer ${talkId}:`, data);
        return data;
    } catch (error) {
        console.error(`Error changing duration for timer ${talkId}:`, error);
        throw error;
    }
}

// Display all timers in the UI
function displayTimers(timersData) {
    const container = document.getElementById('timers-container');
    container.innerHTML = '';

    // The API returns { status: {...}, timerInfo: [...] }
    const timers = timersData.timerInfo || [];

    if (timers.length === 0) {
        container.innerHTML = '<p class="no-timers">No timers available</p>';
        return;
    }

    // Get current running timer ID from status
    const runningTimerId = timersData.status?.talkId;
    const isAnyRunning = timersData.status?.isRunning;

    timers.forEach(timer => {
        createTimerCard(timer, runningTimerId, isAnyRunning);
    });

    // Store data for local updates
    allTimersData = timersData;
    lastUpdateTime = Date.now();
}

// Create a timer card in the DOM
function createTimerCard(timer, runningTimerId, isAnyRunning) {
    const container = document.getElementById('timers-container');
    const isRunning = isAnyRunning && timer.talkId === runningTimerId;

    const timerCard = document.createElement('div');
    timerCard.className = `timer-card ${isRunning ? 'running' : ''}`;
    timerCard.id = `timer-${timer.talkId}`;

    // Store timer data as attributes for local updates
    timerCard.dataset.talkId = timer.talkId;
    timerCard.dataset.isRunning = isRunning ? 'true' : 'false';
    timerCard.dataset.completedTimeSecs = timer.completedTimeSecs || 0;

    const duration = formatDuration(timer.modifiedDurationSecs || timer.originalDurationSecs || 0);
    const elapsed = formatDuration(timer.completedTimeSecs || 0);
    const status = isRunning ? 'Running' : 'Stopped';

    timerCard.innerHTML = `
        <div class="timer-header">
            <h3>${timer.talkTitle}</h3>
            <span class="timer-status ${isRunning ? 'status-running' : 'status-stopped'}">${status}</span>
        </div>
        <div class="timer-details">
            <div class="timer-info">
                <span class="label">Talk ID:</span>
                <span class="value">${timer.talkId}</span>
            </div>
            <div class="timer-info">
                <span class="label">Section:</span>
                <span class="value">${timer.meetingSectionNameLocalised || 'N/A'}</span>
            </div>
            <div class="timer-info">
                <span class="label">Duration:</span>
                <span class="value">${duration}</span>
            </div>
            <div class="timer-info">
                <span class="label">Timer:</span>
                <span class="timer-elapsed" class="value">${elapsed}</span>
            </div>
        </div>
        <div class="timer-controls">
            <button onclick="handleStartTimer(${timer.talkId})" ${isRunning ? 'disabled' : ''}>Start</button>
            <button onclick="handleStopTimer(${timer.talkId})" ${!isRunning ? 'disabled' : ''}>Stop</button>            
        </div>
    `;

    container.appendChild(timerCard);
}

// Update a specific timer in the DOM
function updateTimerCard(timerInfo, status) {
    const timerCard = document.getElementById(`timer-${timerInfo.talkId}`);
    if (!timerCard) return;

    console.log('updateTimerCard - timerInfo.talkId:', timerInfo.talkId);
    console.log('updateTimerCard - status:', status);
    console.log('updateTimerCard - status.isRunning:', status.isRunning);
    console.log('updateTimerCard - status.talkId:', status.talkId);

    const isRunning = status.isRunning && status.talkId === timerInfo.talkId;
    console.log('updateTimerCard - isRunning:', isRunning);

    // Update class
    timerCard.className = `timer-card ${isRunning ? 'running' : ''}`;

    // Update data attributes
    timerCard.dataset.isRunning = isRunning ? 'true' : 'false';
    timerCard.dataset.completedTimeSecs = timerInfo.completedTimeSecs || 0;

    // Update status display
    const statusSpan = timerCard.querySelector('.timer-status');
    statusSpan.textContent = isRunning ? 'Running' : 'Stopped';
    statusSpan.className = `timer-status ${isRunning ? 'status-running' : 'status-stopped'}`;

    // Update elapsed time
    const elapsedSpan = timerCard.querySelector('.timer-elapsed');
    elapsedSpan.textContent = formatDuration(timerInfo.completedTimeSecs || 0);

    // Update buttons
    const buttons = timerCard.querySelectorAll('.timer-controls button');
    console.log('updateTimerCard - buttons found:', buttons.length);
    console.log('updateTimerCard - setting Start button disabled to:', isRunning);
    console.log('updateTimerCard - setting Stop button disabled to:', !isRunning);

    buttons[0].disabled = isRunning; // Start button
    buttons[1].disabled = !isRunning; // Stop button

    console.log('updateTimerCard - Start button disabled after:', buttons[0].disabled);
    console.log('updateTimerCard - Stop button disabled after:', buttons[1].disabled);
}

// Update elapsed time displays locally (without API calls)
function updateElapsedTimes() {
    if (!allTimersData || !lastUpdateTime) return;

    const now = Date.now();
    const elapsedMs = now - lastUpdateTime;
    const elapsedSecs = Math.floor(elapsedMs / 1000);

    const timerCards = document.querySelectorAll('.timer-card');
    timerCards.forEach(card => {
        const elapsedSpan = card.querySelector('.timer-elapsed');
        if (elapsedSpan) {
            const isRunning = card.dataset.isRunning === 'true';
            const currentElapsed = isRunning ? elapsedSecs : 0;

            elapsedSpan.textContent = formatDuration(currentElapsed);
        }
    });
}

// Format duration in seconds to mm:ss
function formatDuration(seconds) {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
}

// Show error message
function showError(message) {
    const errorDiv = document.getElementById('error');
    errorDiv.textContent = `Error: ${message}`;
    errorDiv.style.display = 'block';
    setTimeout(() => {
        errorDiv.style.display = 'none';
    }, 5000);
}

// Hide loading indicator
function hideLoading() {
    const loadingDiv = document.getElementById('loading');
    loadingDiv.style.display = 'none';
}

// Event handlers
async function handleStartTimer(talkId) {
    const timerCard = document.getElementById(`timer-${talkId}`);
    if (!timerCard) return;

    // Disable both buttons immediately to prevent double-clicks
    const buttons = timerCard.querySelectorAll('.timer-controls button');
    buttons.forEach(btn => btn.disabled = true);

    try {
        // Wait 1500ms before making the API call to let any pending operations settle
        await new Promise(resolve => setTimeout(resolve, 1500));

        const result = await startTimer(talkId);
        console.log('Start timer response:', result);

        if (result && result.success) {
            // Use currentStatus from the response (it's more up-to-date)
            const currentStatus = result.currentStatus;
            console.log('Current status from response:', currentStatus);

            // Fetch all timers to get timerInfo, but update status from currentStatus
            const fullData = await getAllTimers();

            if (fullData && fullData.timerInfo) {
                // Override the status with the authoritative currentStatus from the start response
                fullData.status = {
                    talkId: currentStatus.talkId,
                    targetSeconds: currentStatus.targetSeconds,
                    isRunning: true, // We just started it, so it's definitely running
                    timeElapsed: currentStatus.timeElapsed,
                    closingSecs: currentStatus.closingSecs
                };

                // Update local state with all timers
                allTimersData = fullData;
                lastUpdateTime = Date.now();

                // Update just this timer's card
                const timer = fullData.timerInfo.find(t => t.talkId === talkId);
                if (timer) {
                    updateTimerCard(timer, fullData.status);
                } else {
                    console.warn('Timer not found in response:', talkId);
                }
            }
        } else {
            console.warn('Start timer failed:', result);
            showError('Failed to start timer');
            // Re-enable buttons on failure
            buttons[0].disabled = false; // Start button
        }
    } catch (error) {
        showError(`Failed to start timer: ${error.message}`);
        // Re-enable buttons on error
        buttons[0].disabled = false; // Start button
    }
}

async function handleStopTimer(talkId) {
    const timerCard = document.getElementById(`timer-${talkId}`);
    if (!timerCard) return;

    // Disable both buttons immediately to prevent double-clicks
    const buttons = timerCard.querySelectorAll('.timer-controls button');
    buttons.forEach(btn => btn.disabled = true);

    try {
        // Wait 1500ms before making the API call to let any pending operations settle
        await new Promise(resolve => setTimeout(resolve, 1500));

        const result = await stopTimer(talkId);
        console.log('Stop timer response:', result);

        if (result && result.success) {
            // Use currentStatus from the response
            const currentStatus = result.currentStatus;

            // Fetch all timers to get timerInfo, but update status from currentStatus
            const fullData = await getAllTimers();

            if (fullData && fullData.timerInfo) {
                // Override the status with the authoritative currentStatus from the stop response
                fullData.status = {
                    talkId: currentStatus.talkId,
                    targetSeconds: currentStatus.targetSeconds,
                    isRunning: false, // We just stopped it, so it's definitely not running
                    timeElapsed: currentStatus.timeElapsed,
                    closingSecs: currentStatus.closingSecs
                };

                // Update local state with all timers
                allTimersData = fullData;
                lastUpdateTime = Date.now();

                // Update just this timer's card
                const timer = fullData.timerInfo.find(t => t.talkId === talkId);
                if (timer) {
                    updateTimerCard(timer, fullData.status);
                } else {
                    console.warn('Timer not found in response:', talkId);
                }
            }
        } else {
            console.warn('Stop timer failed:', result);
            showError('Failed to stop timer');
            // Re-enable buttons on failure
            buttons[1].disabled = false; // Stop button
        }
    } catch (error) {
        showError(`Failed to stop timer: ${error.message}`);
        // Re-enable buttons on error
        buttons[1].disabled = false; // Stop button
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', async () => {
    try {
        const timers = await getAllTimers();
        hideLoading();
        displayTimers(timers);

        // Update elapsed time display every 200ms (smooth without API calls)
        setInterval(updateElapsedTimes, 200);
    } catch (error) {
        hideLoading();
        showError(`Failed to load timers: ${error.message}`);
    }
});