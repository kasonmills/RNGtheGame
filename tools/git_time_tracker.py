#!/usr/bin/env python3
"""
Hybrid Time Tracking System for Game Development
Automatically analyzes Git commits to track coding time.
Integrates with Excel spreadsheet for complete project time tracking.

Usage:
    python3 git_time_tracker.py /path/to/repo

    Or place this script in your game project folder and run:
    python3 git_time_tracker.py .
"""

import subprocess
import json
from datetime import datetime, timedelta
import sys
from pathlib import Path

def get_commit_history(repo_path="."):
    """
    Extract commit history from git repository.
    Returns list of commits with timestamps.
    """
    try:
        # Get all commits with detailed info
        result = subprocess.run(
            ["git", "log", "--format=%ai|%an|%s|%H"],
            cwd=repo_path,
            capture_output=True,
            text=True,
            check=True
        )

        commits = []
        for line in result.stdout.strip().split('\n'):
            if not line or '|' not in line:
                continue

            try:
                parts = line.split('|')
                if len(parts) >= 4:
                    # Parse timestamp: "2024-08-20 14:30:45 +0000"
                    datetime_str = parts[0].strip()
                    timestamp = datetime.fromisoformat(datetime_str.split('+')[0].strip())

                    commits.append({
                        'timestamp': timestamp,
                        'author': parts[1].strip(),
                        'message': parts[2].strip(),
                        'hash': parts[3].strip()[:7]  # Short hash
                    })
            except Exception as e:
                print(f"  Warning: Could not parse commit: {line[:50]}... ({e})")
                pass

        # Sort by timestamp (oldest first)
        commits.sort(key=lambda x: x['timestamp'])
        return commits

    except subprocess.CalledProcessError as e:
        print(f"❌ Error reading git repository: {e}")
        print("Make sure you're running this in a Git repository.")
        return []


def estimate_sessions(commits, session_gap_minutes=120):
    """
    Group commits into coding sessions.
    If commits are more than session_gap_minutes apart, they're separate sessions.

    Default: 2 hours between commits = new session
    """
    if not commits:
        return []

    sessions = []
    current_session = [commits[0]]

    for i in range(1, len(commits)):
        prev_commit = current_session[-1]
        curr_commit = commits[i]

        time_diff = (curr_commit['timestamp'] - prev_commit['timestamp']).total_seconds() / 60

        if time_diff > session_gap_minutes:
            # Gap too long, end session and start new one
            sessions.append(current_session)
            current_session = [curr_commit]
        else:
            # Continue current session
            current_session.append(curr_commit)

    # Don't forget the last session
    if current_session:
        sessions.append(current_session)

    return sessions


def calculate_session_duration(session):
    """
    Calculate realistic session duration.

    Uses:
    1. Time span from first to last commit
    2. Estimated time per commit (8-12 min based on commit frequency)
    3. Caps at 8 hours per session (reasonable max for focused work)
    """
    if not session:
        return 0

    first_commit = session[0]
    last_commit = session[-1]

    # Actual elapsed time from first to last commit
    duration_minutes = (last_commit['timestamp'] - first_commit['timestamp']).total_seconds() / 60

    # If commits are close together, estimate based on commit count
    # Each commit = ~8-12 minutes of work (includes thinking, testing, etc.)
    num_commits = len(session)
    commits_estimated_minutes = num_commits * 10  # 10 min average per commit

    # Take the maximum of actual duration or commit-based estimate
    estimated_minutes = max(duration_minutes, commits_estimated_minutes)

    # Minimum 30 minutes per session, maximum 8 hours
    estimated_minutes = max(30, min(estimated_minutes, 480))

    estimated_hours = estimated_minutes / 60

    return round(estimated_hours, 2)


def generate_report(sessions):
    """
    Print a human-readable session report.
    """
    print("\n" + "="*70)
    print("📊 ESTIMATED CODING SESSIONS FROM GIT HISTORY")
    print("="*70 + "\n")

    total_hours = 0

    for i, session in enumerate(sessions, 1):
        duration = calculate_session_duration(session)
        total_hours += duration

        first_time = session[0]['timestamp']
        last_time = session[-1]['timestamp']
        commit_count = len(session)

        print(f"Session {i}:")
        print(f"  📅 Date:       {first_time.strftime('%A, %B %d, %Y')}")
        print(f"  ⏰ Time:       {first_time.strftime('%H:%M')} → {last_time.strftime('%H:%M')}")
        print(f"  📝 Commits:    {commit_count}")
        print(f"  ⏱️  Duration:    {duration} hours")

        # Show first and last commit for context
        first_msg = session[0]['message'][:50]
        last_msg = session[-1]['message'][:50]
        print(f"  📌 First:      {first_msg}...")
        print(f"  📌 Last:       {last_msg}...")
        print()

    print("="*70)
    print(f"✅ Total Estimated Coding Hours: {total_hours:.2f}")
    print("="*70 + "\n")

    print("💡 NOTES:")
    print("  • These are estimates based on commit patterns")
    print("  • You can adjust individual session hours in the spreadsheet")
    print("  • The spreadsheet tracks both coding AND business tasks\n")

    return sessions, total_hours


def export_to_json(sessions, output_file="coding_sessions.json"):
    """
    Export sessions to JSON format (for reference or integration).
    """
    data = []
    for i, session in enumerate(sessions, 1):
        duration = calculate_session_duration(session)
        first_time = session[0]['timestamp']
        last_time = session[-1]['timestamp']

        data.append({
            'session_number': i,
            'date': first_time.strftime('%Y-%m-%d'),
            'start_time': first_time.strftime('%H:%M'),
            'end_time': last_time.strftime('%H:%M'),
            'commits': len(session),
            'estimated_hours': duration,
            'first_commit': session[0]['message'],
            'last_commit': session[-1]['message']
        })

    with open(output_file, 'w') as f:
        json.dump(data, f, indent=2)

    return data


def print_instructions():
    """
    Print instructions for using the tracker.
    """
    print("\n" + "="*70)
    print("📋 HOW TO USE THIS SCRIPT")
    print("="*70 + "\n")

    print("1️⃣  SETUP (First time only):")
    print("   • Place this script in your game project folder")
    print("   • Make sure your project is a Git repository")
    print("   • Run: python3 git_time_tracker.py .\n")

    print("2️⃣  WHAT IT DOES:")
    print("   • Reads all your Git commits")
    print("   • Groups commits into coding sessions (2+ hour gaps = new session)")
    print("   • Estimates hours per session based on commit patterns")
    print("   • Exports results to coding_sessions.json\n")

    print("3️⃣  INTEGRATE WITH SPREADSHEET:")
    print("   • Open Game_Dev_Time_Tracking.xlsx")
    print("   • Go to 'Coding Sessions' sheet")
    print("   • Manually copy estimated hours from output below")
    print("   • OR use the coding_sessions.json file (import instructions coming)\n")

    print("4️⃣  ONGOING TRACKING:")
    print("   • Every time you finish coding, commit your changes to Git")
    print("   • Run this script periodically (weekly/bi-weekly)")
    print("   • Update the spreadsheet with new session estimates")
    print("   • Manually log business tasks (marketing, legal, etc.) separately\n")

    print("5️⃣  ADJUST IF NEEDED:")
    print("   • If the auto-estimate is wrong, edit the 'Adjusted Hrs' column")
    print("   • Example: If it estimates 2 hrs but you worked 3, change it to 3\n")

    print("="*70 + "\n")


if __name__ == "__main__":
    # Get repo path from command line or use current directory
    repo_path = sys.argv[1] if len(sys.argv) > 1 else "."

    print("\n🔍 Analyzing Git repository...\n")

    # Read commits
    commits = get_commit_history(repo_path)

    if not commits:
        print("❌ No commits found. Make sure you're in a Git repository.")
        sys.exit(1)

    print(f"✅ Found {len(commits)} commits\n")

    # Estimate sessions
    sessions = estimate_sessions(commits, session_gap_minutes=120)
    print(f"✅ Grouped into {len(sessions)} coding sessions\n")

    # Generate report
    sessions_data, total_hours = generate_report(sessions)

    # Export to JSON
    json_file = "coding_sessions.json"
    export_to_json(sessions, json_file)
    print(f"✅ Exported to: {json_file}")
    print(f"   (Use this for reference or spreadsheet import)\n")

    # Print how to use
    print_instructions()

    print("🎮 Next Steps:")
    print("   1. Review the sessions above")
    print("   2. Open Game_Dev_Time_Tracking.xlsx")
    print("   3. Update 'Coding Sessions' sheet with estimates")
    print("   4. Add any business/marketing tasks to 'Business Tasks' sheet")
    print("   5. Review Summary sheet for total investment\n")
