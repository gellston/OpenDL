
# VTDEEP Manual


#### 라이브러리 사용법

* C++  


```cpp

#include <vtfactory.h>

void main(){
    // Classifier 예제
    // Classifier 초기화
    vt::Iclassifier * classifier = createClassifier("모델파일 경로", "모델  정보 파일");
    // Classifier 실행
    classifier->run(unsigned image 버퍼 주소)
    // Classifier 최대 스코어 확인
    std::cout << "Max score = " << classifier->getMaxScore() << std::endl;
    // Classifier 최대 인덱스 확인
    std::cout << "Max score Index = " << classifier->getMaxScoreIndex() << std::endl;
    // Classifier 인덱스 갯수 확인
    std::cout << "Max score Index = " << classifier->getLabelCount() << std::endl;
    // Classifier 최대 인덱스 확인
    std::cout << "Max score Index = " << classifier->getMaxScoreIndex() << std::endl;
    // Clsasifier 모든 스코어 확인
    for(int index =0; index < classifier->getLabelCount(); index++)
        std::cout << "Max score Index = " << classifier->getScore(index) << std::endl;
}


```

#### 업데이트 예정 사항

 1. ~~Classifier 모델 추가 VTLight_Color 추가 (ver 1.0)~~
 2. ~~Classifier 모델 추가 VTLight_Gray 추가 (ver 1.0)~~
 3. 지속적인 분류 모델 추가
 4. tensorflow python 모델 추가 기능
 5. segmentation을 위한 UI 추가
 6. 메뉴얼 업데이트 - c++ 라이브러리 설치 방법 
 7. Example 코드 추가 
 8. C# 라이브러리 추가
 9. ~~로그인 기능 추가~~
 10. ~~모델 프리징 및 패키징 기능 추가 (ver 1.0)~~
 11. 메뉴얼 업데이트 - 메뉴얼 양식 변경

---
#### 1.0 Release Note

* 업데이트 리스트  

```
1. Classification 모델 VTLightNet_Color 추가 
   - 입력 이미지 사이즈 : 100x100x3
   - 아웃풋 라벨 갯수 : 2
   
2. Classification 모델 VTLightNet_Gray 추가 
   - 입력 이미지 사이즈 : 100x100x1
   - 아웃풋 라벨 갯수 : 2
  
3. 모델 프리징 및 패키징 기능 추가.

4. 로그인 기능 추가 

```
---