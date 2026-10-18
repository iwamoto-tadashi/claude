import React, { useState, useEffect } from 'react';
import { CheckCircle2, Circle, Play, Pause, RotateCcw, Timer, Dumbbell, Heart, Target } from 'lucide-react';

const HomeWorkoutPlanner = () => {
  const [activeDay, setActiveDay] = useState(0);
  const [completedExercises, setCompletedExercises] = useState({});
  const [isTimerRunning, setIsTimerRunning] = useState(false);
  const [currentTimer, setCurrentTimer] = useState(0);
  const [timerInterval, setTimerInterval] = useState(null);

  const weeklyPlan = [
    {
      day: "月曜日",
      type: "筋トレ重点",
      duration: "60分",
      exercises: [
        { name: "ウォームアップ（軽いストレッチ）", time: "5分", sets: "1セット" },
        { name: "プランク", time: "30秒×3セット", sets: "3セット" },
        { name: "腕立て伏せ（膝つきOK）", time: "10回×3セット", sets: "3セット" },
        { name: "スクワット", time: "15回×3セット", sets: "3セット" },
        { name: "壁腕立て伏せ", time: "15回×2セット", sets: "2セット" },
        { name: "腹筋運動（クランチ）", time: "15回×3セット", sets: "3セット" },
        { name: "背筋運動（うつ伏せ）", time: "10回×2セット", sets: "2セット" },
        { name: "クールダウン（ストレッチ）", time: "10分", sets: "1セット" }
      ]
    },
    {
      day: "火曜日",
      type: "有酸素＋ヨガ",
      duration: "60分",
      exercises: [
        { name: "軽いストレッチ", time: "5分", sets: "1セット" },
        { name: "その場ステップ運動", time: "10分", sets: "1セット" },
        { name: "ヨガ基本ポーズ", time: "20分", sets: "1セット" },
        { name: "椅子昇降運動", time: "5分×2回", sets: "2セット" },
        { name: "バランスポーズ", time: "5分", sets: "1セット" },
        { name: "深いストレッチ", time: "15分", sets: "1セット" }
      ]
    },
    {
      day: "水曜日",
      type: "筋トレ重点",
      duration: "60分",
      exercises: [
        { name: "ウォームアップ", time: "5分", sets: "1セット" },
        { name: "プランク（サイドプランク含む）", time: "30秒×4セット", sets: "4セット" },
        { name: "スクワット＋カーフレイズ", time: "12回×3セット", sets: "3セット" },
        { name: "腕立て伏せバリエーション", time: "8回×3セット", sets: "3セット" },
        { name: "ランジ（その場）", time: "10回×2セット", sets: "2セット" },
        { name: "腹筋サーキット", time: "30秒×4種目", sets: "4セット" },
        { name: "ストレッチ", time: "12分", sets: "1セット" }
      ]
    },
    {
      day: "木曜日",
      type: "軽い有酸素",
      duration: "45分",
      exercises: [
        { name: "ストレッチ", time: "8分", sets: "1セット" },
        { name: "ゆっくりステップ運動", time: "15分", sets: "1セット" },
        { name: "椅子を使った運動", time: "10分", sets: "1セット" },
        { name: "リラックスヨガ", time: "12分", sets: "1セット" }
      ]
    },
    {
      day: "金曜日",
      type: "筋トレ重点",
      duration: "60分",
      exercises: [
        { name: "ウォームアップ", time: "5分", sets: "1セット" },
        { name: "プランク＋腕立て", time: "30秒+10回×3", sets: "3セット" },
        { name: "スクワット＋ジャンプ（静音）", time: "15回×3セット", sets: "3セット" },
        { name: "壁を使った逆立ち（足上げ）", time: "30秒×2セット", sets: "2セット" },
        { name: "全身筋トレサーキット", time: "20分", sets: "1セット" },
        { name: "ストレッチ", time: "10分", sets: "1セット" }
      ]
    },
    {
      day: "土曜日",
      type: "ヨガ＋ストレッチ",
      duration: "60分",
      exercises: [
        { name: "朝のヨガルーティン", time: "30分", sets: "1セット" },
        { name: "全身ストレッチ", time: "20分", sets: "1セット" },
        { name: "呼吸法＋瞑想", time: "10分", sets: "1セット" }
      ]
    },
    {
      day: "日曜日",
      type: "休息日",
      duration: "30分",
      exercises: [
        { name: "軽いストレッチ", time: "15分", sets: "1セット" },
        { name: "リラックス時間", time: "15分", sets: "1セット" }
      ]
    }
  ];

  const toggleExercise = (dayIndex, exerciseIndex) => {
    const key = `${dayIndex}-${exerciseIndex}`;
    setCompletedExercises(prev => ({
      ...prev,
      [key]: !prev[key]
    }));
  };

  const startTimer = (minutes) => {
    if (timerInterval) {
      clearInterval(timerInterval);
    }
    setCurrentTimer(minutes * 60);
    setIsTimerRunning(true);
    
    const interval = setInterval(() => {
      setCurrentTimer(prev => {
        if (prev <= 1) {
          setIsTimerRunning(false);
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    
    setTimerInterval(interval);
  };

  const stopTimer = () => {
    if (timerInterval) {
      clearInterval(timerInterval);
      setTimerInterval(null);
    }
    setIsTimerRunning(false);
  };

  const resetTimer = () => {
    stopTimer();
    setCurrentTimer(0);
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const getCompletedCount = (dayIndex) => {
    return weeklyPlan[dayIndex].exercises.filter((_, exerciseIndex) => 
      completedExercises[`${dayIndex}-${exerciseIndex}`]
    ).length;
  };

  const getProgressPercentage = (dayIndex) => {
    const total = weeklyPlan[dayIndex].exercises.length;
    const completed = getCompletedCount(dayIndex);
    return Math.round((completed / total) * 100);
  };

  return (
    <div className="max-w-4xl mx-auto p-6 bg-gradient-to-br from-blue-50 to-indigo-100 min-h-screen">
      <div className="bg-white rounded-xl shadow-lg overflow-hidden">
        {/* ヘッダー */}
        <div className="bg-gradient-to-r from-blue-600 to-indigo-700 text-white p-6">
          <h1 className="text-3xl font-bold flex items-center gap-3">
            <Dumbbell className="h-8 w-8" />
            在宅ワーク向け静音運動プラン
          </h1>
          <p className="mt-2 text-blue-100">賃貸でも安心！音を出さない効果的な運動メニュー</p>
        </div>

        {/* タイマー */}
        <div className="bg-gray-50 p-4 border-b">
          <div className="flex items-center justify-center gap-4">
            <div className="bg-white rounded-lg p-4 shadow">
              <div className="text-2xl font-mono font-bold text-center text-gray-800">
                {formatTime(currentTimer)}
              </div>
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => startTimer(5)}
                className="flex items-center gap-2 px-3 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors"
              >
                <Timer className="h-4 w-4" />
                5分
              </button>
              <button
                onClick={() => startTimer(10)}
                className="flex items-center gap-2 px-3 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
              >
                <Timer className="h-4 w-4" />
                10分
              </button>
              <button
                onClick={isTimerRunning ? stopTimer : () => startTimer(1)}
                className="flex items-center gap-2 px-3 py-2 bg-orange-500 text-white rounded-lg hover:bg-orange-600 transition-colors"
              >
                {isTimerRunning ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
                {isTimerRunning ? '停止' : 'スタート'}
              </button>
              <button
                onClick={resetTimer}
                className="flex items-center gap-2 px-3 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition-colors"
              >
                <RotateCcw className="h-4 w-4" />
                リセット
              </button>
            </div>
          </div>
        </div>

        {/* 曜日タブ */}
        <div className="flex overflow-x-auto bg-gray-100">
          {weeklyPlan.map((day, index) => (
            <button
              key={index}
              onClick={() => setActiveDay(index)}
              className={`flex-shrink-0 px-6 py-4 text-center border-b-2 transition-colors ${
                activeDay === index
                  ? 'border-blue-600 bg-white text-blue-600 font-semibold'
                  : 'border-transparent text-gray-600 hover:text-gray-800'
              }`}
            >
              <div className="text-sm font-medium">{day.day}</div>
              <div className="text-xs text-gray-500">{day.type}</div>
              <div className="text-xs mt-1">
                <span className="inline-block w-2 h-2 rounded-full bg-green-400 mr-1"></span>
                {getProgressPercentage(index)}%
              </div>
            </button>
          ))}
        </div>

        {/* 当日の運動内容 */}
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-800">{weeklyPlan[activeDay].day}の運動</h2>
              <div className="flex items-center gap-4 mt-2 text-sm text-gray-600">
                <span className="flex items-center gap-1">
                  <Target className="h-4 w-4" />
                  {weeklyPlan[activeDay].type}
                </span>
                <span className="flex items-center gap-1">
                  <Timer className="h-4 w-4" />
                  目安時間: {weeklyPlan[activeDay].duration}
                </span>
              </div>
            </div>
            <div className="text-right">
              <div className="text-sm text-gray-600">進捗</div>
              <div className="text-2xl font-bold text-blue-600">
                {getCompletedCount(activeDay)}/{weeklyPlan[activeDay].exercises.length}
              </div>
            </div>
          </div>

          {/* エクササイズリスト */}
          <div className="space-y-3">
            {weeklyPlan[activeDay].exercises.map((exercise, exerciseIndex) => {
              const isCompleted = completedExercises[`${activeDay}-${exerciseIndex}`];
              return (
                <div
                  key={exerciseIndex}
                  className={`p-4 rounded-lg border-2 transition-all cursor-pointer ${
                    isCompleted
                      ? 'border-green-300 bg-green-50'
                      : 'border-gray-200 bg-white hover:border-blue-300 hover:bg-blue-50'
                  }`}
                  onClick={() => toggleExercise(activeDay, exerciseIndex)}
                >
                  <div className="flex items-center gap-3">
                    {isCompleted ? (
                      <CheckCircle2 className="h-6 w-6 text-green-600 flex-shrink-0" />
                    ) : (
                      <Circle className="h-6 w-6 text-gray-400 flex-shrink-0" />
                    )}
                    <div className="flex-1">
                      <div className={`font-semibold ${isCompleted ? 'text-green-800 line-through' : 'text-gray-800'}`}>
                        {exercise.name}
                      </div>
                      <div className="text-sm text-gray-600 mt-1">
                        <span className="inline-block mr-4">⏱️ {exercise.time}</span>
                        <span className="inline-block">🔢 {exercise.sets}</span>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          {/* 運動のコツ */}
          <div className="mt-8 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <h3 className="font-semibold text-yellow-800 mb-2">💡 在宅運動のコツ</h3>
            <ul className="text-sm text-yellow-700 space-y-1">
              <li>• 運動前後のストレッチを必ず行いましょう</li>
              <li>• ヨガマットがあると床での運動が快適です</li>
              <li>• 水分補給を忘れずに</li>
              <li>• 無理をせず、徐々に強度を上げていきましょう</li>
              <li>• 音が気になる場合は、厚手のマットやタオルを敷きましょう</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomeWorkoutPlanner;