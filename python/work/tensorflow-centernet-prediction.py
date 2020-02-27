import numpy as np
import cv2 as cv2

import tensorflow as tf
import os
import time

from util.helper import extract_box_from_featuremaps_with_score
from util.helper import filter_overlap_boxes



sess=tf.compat.v1.Session()
saver = tf.compat.v1.train.import_meta_graph('C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_atrous_centernet(celba_dataloader)/VTCenternet.meta')
saver.restore(sess,tf.compat.v1.train.latest_checkpoint('C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_atrous_centernet(celba_dataloader)/'))

graph = tf.get_default_graph()
phase = graph.get_tensor_by_name("CenterNet/phase:0")
X = graph.get_tensor_by_name("CenterNet/X:0")
Heatmap = graph.get_tensor_by_name("CenterNet/detector/heatmap:0")
Sizemap = graph.get_tensor_by_name("CenterNet/detector/sizemap:0")


camera = cv2.VideoCapture(0)

### FPS CODE
start_time = time.time()
x = 1 # displays the frame rate every 1 second
counter = 0
### FPS CODE

cv2.namedWindow('image', cv2.WINDOW_NORMAL)
cv2.resizeWindow('image', 1280, 1024)

originalBatch = []

ret_val, img = camera.read()

# crop = img[100:480 - 100, 200:640 - 200]
resize = cv2.resize(img, dsize=(256, 256), interpolation=cv2.INTER_AREA)
originalBatch.append(resize)

while True:


    heatmap , sizemap = sess.run([Heatmap, Sizemap], feed_dict={X: originalBatch, phase: False})
    box_list = extract_box_from_featuremaps_with_score(128, 128, 2, heatmap[0], sizemap[0], 0.5)
    final_box_list = filter_overlap_boxes(box_list, 0.5)

    for box in box_list:
        cv2.rectangle(resize, (box[0], box[1]), (box[2], box[3]), (0, 0, 255), 1)
        break

    cv2.imshow('image', resize)

    if cv2.waitKey(1) == 27:
        break  # esc to quit

    ### FPS CODE
    counter += 1
    if (time.time() - start_time) > x:
        print("FPS: ", counter / (time.time() - start_time))
        counter = 0
        start_time = time.time()
    ### FPS CODE